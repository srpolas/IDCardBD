using System.Text;
using System.Text.Json;
using IDCardBD.Web.Models;
using QRCoder;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IDCardBD.Web.Services
{
    public class PdfService : IPdfService
    {
        // CR80 card dimensions in points: 85.6mm x 53.98mm
        public const float CardWidth = 242.64f;
        public const float CardHeight = 153f;

        private const string DefaultFontFamily = "Lato";

        private static readonly object FontLock = new();
        private static bool _fontsRegistered;

        private readonly IWebHostEnvironment _environment;

        public PdfService(IWebHostEnvironment environment)
        {
            _environment = environment;
            EnsureFontsRegistered();
        }

        public byte[] GenerateIdCard(IdentityBase person, CardTemplate template)
        {
            var portrait = string.Equals(template.Orientation, "Portrait", StringComparison.OrdinalIgnoreCase);
            var cardW = portrait ? CardHeight : CardWidth;
            var cardH = portrait ? CardWidth : CardHeight;

            var frontElements = ParseElements(template.FrontElementsJson);
            var backElements = ParseElements(template.BackElementsJson);
            var qrPng = GenerateQrCodeImage(person, template);

            byte[]? PhotoBytes() => LoadPhotoBytes(person.PhotoPath);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(new PageSize(cardW, cardH));
                    page.Margin(0);
                    RenderSide(page, template.FrontBgPath, frontElements, person, template, qrPng, PhotoBytes, cardW, cardH);
                });

                container.Page(page =>
                {
                    page.Size(new PageSize(cardW, cardH));
                    page.Margin(0);
                    RenderSide(page, template.BackBgPath, backElements, person, template, qrPng, PhotoBytes, cardW, cardH);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateQrCodeImage(IdentityBase person, CardTemplate template)
        {
            var vCard = BuildVCard(person, template);
            var data = new QRCodeGenerator().CreateQrCode(vCard, QRCodeGenerator.ECCLevel.M);
            return new PngByteQRCode(data).GetGraphic(10);
        }

        private void RenderSide(PageDescriptor page, string? bgPath, List<CardElement> elements,
            IdentityBase person, CardTemplate template, byte[] qrPng, Func<byte[]?> loadPhotoBytes,
            float cardW, float cardH)
        {
            page.PageColor(Colors.White);

            page.Content().Layers(layers =>
            {
                var bg = ResolvePath(bgPath);
                if (bg != null)
                {
                    // Stretch the background to fill the entire card, ignoring aspect ratio.
                    layers.Layer().Image(bg).FitUnproportionally();
                }

                foreach (var el in elements.Where(e => e.Visible))
                {
                    RenderElement(layers.Layer(), el, person, template, qrPng, loadPhotoBytes, cardW, cardH);
                }

                layers.PrimaryLayer();
            });
        }

        private void RenderElement(IContainer container, CardElement el, IdentityBase person,
            CardTemplate template, byte[] qrPng, Func<byte[]?> loadPhotoBytes, float cardW, float cardH)
        {
            var x = (float)(el.X / 100.0 * cardW);
            var y = (float)(el.Y / 100.0 * cardH);

            container = container.PaddingLeft(x).PaddingTop(y);

            switch (el.Type)
            {
                case "photo":
                {
                    var w = (float)(el.W / 100.0 * cardW);
                    var h = (float)(el.H / 100.0 * cardH);
                    if (w <= 0 || h <= 0) return;
                    var bytes = loadPhotoBytes();
                    if (bytes == null) return;
                    container.Width(w).Height(h).Image(bytes).FitArea();
                    break;
                }
                case "qr":
                {
                    var w = (float)(el.W / 100.0 * cardW);
                    if (w <= 0) return;
                    container.Width(w).Height(w)
                        .Background(Colors.White)
                        .Padding(2)
                        .Image(qrPng).FitArea();
                    break;
                }
                default:
                {
                    var value = el.Field == "custom"
                        ? el.Text
                        : GetFieldValue(el.Field, person, template);
                    if (string.IsNullOrEmpty(value)) return;

                    var remaining = cardW - x;
                    var w = el.W > 0 ? (float)(el.W / 100.0 * cardW) : remaining;
                    if (w > remaining) w = remaining;
                    if (w <= 1) return;

                    var text = container.Width(w);
                    if (el.Align == "center") text = text.AlignCenter();
                    else if (el.Align == "right") text = text.AlignRight();

                    if (el.Rotation != 0) text = text.Rotate((float)el.Rotation);

                    var family = string.IsNullOrWhiteSpace(el.FontFamily) ? DefaultFontFamily : el.FontFamily;
                    var textStyle = text.Text(value)
                        .FontFamily(family)
                        .FontSize((float)el.FontSize)
                        .FontColor(el.Color)
                        .LineHeight(1.1f);

                    if (el.Bold) textStyle.Bold();
                    break;
                }
            }
        }

        // ---------------------------------------------------------------------
        // Field map: designer field keys -> concrete values for the PDF.
        // Keep in sync with the field list in Views/Design/Designer.cshtml.
        // ---------------------------------------------------------------------
        private static string? GetFieldValue(string? field, IdentityBase person, CardTemplate template)
        {
            if (string.IsNullOrEmpty(field)) return null;

            var student = person as Student;
            var employee = person as Employee;
            var teacher = person as Teacher;

            return field switch
            {
                "FullName" => person.FullName,
                "IdNumber" => person.IdNumber,
                "Gender" => person.Gender,
                "IssuedOn" => FmtDate(person.IssuedOn),
                "ValidUntil" => FmtDate(person.ValidUntil),
                "Address" => person.Address,
                "InstituteName" => template.InstituteName,

                "Phone" => employee?.PhoneNumber ?? teacher?.PhoneNumber ?? student?.PhoneNumber,
                "Email" => employee?.Email ?? teacher?.Email ?? student?.Email,
                "BloodGroup" => employee?.BloodGroup ?? teacher?.BloodGroup ?? student?.BloodGroup,
                "CardCode" => student?.RollNumber ?? employee?.EmployeeCode ?? teacher?.TeacherCode,

                "RollNumber" => student?.RollNumber,
                "ClassName" => student?.Class?.Name,
                "SectionName" => student?.Section?.Name,
                "GroupName" => student?.Group?.Name,
                "DateOfBirth" => FmtDate(student?.DateOfBirth),
                "Session" => student?.Session,
                "AdmissionDate" => FmtDate(student?.AdmissionDate),
                "FathersName" => student?.FathersName,
                "MothersName" => student?.MothersName,
                "GuardianName" => student?.GuardianName,

                "Code" => employee?.EmployeeCode ?? teacher?.TeacherCode,
                "Designation" => employee?.Designation ?? teacher?.Designation,
                "Department" => employee?.Department ?? teacher?.Department,
                "Location" => employee?.Location ?? teacher?.Location,
                "WorkingArea" => employee?.WorkingArea ?? teacher?.WorkingArea,
                "EmergencyContact" => employee?.EmergencyContact ?? teacher?.EmergencyContact,

                _ => null
            };
        }

        private static string? FmtDate(DateTime? date) => date?.ToString("dd/MM/yyyy");

        private static List<CardElement> ParseElements(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<CardElement>();
            try
            {
                return JsonSerializer.Deserialize<List<CardElement>>(json) ?? new List<CardElement>();
            }
            catch (JsonException)
            {
                return new List<CardElement>();
            }
        }

        private string? ResolvePath(string? webPath)
        {
            if (string.IsNullOrWhiteSpace(webPath)) return null;
            var full = Path.Combine(_environment.WebRootPath, webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(full) ? full : null;
        }

        private byte[]? LoadPhotoBytes(string? photoPath)
        {
            if (string.IsNullOrWhiteSpace(photoPath)) return null;
            var full = Path.Combine(_environment.WebRootPath, photoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(full) ? File.ReadAllBytes(full) : null;
        }

        private static string BuildVCard(IdentityBase person, CardTemplate template)
        {
            var student = person as Student;
            var employee = person as Employee;
            var teacher = person as Teacher;

            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCARD");
            sb.AppendLine("VERSION:3.0");
            sb.AppendLine($"N:{Escape(person.FullName)};;;;");
            sb.AppendLine($"FN:{Escape(person.FullName)}");

            if (!string.IsNullOrWhiteSpace(template.InstituteName))
                sb.AppendLine($"ORG:{Escape(template.InstituteName)}");

            var phone = employee?.PhoneNumber ?? teacher?.PhoneNumber ?? student?.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(phone)) sb.AppendLine($"TEL;TYPE=CELL:{phone}");

            var email = employee?.Email ?? teacher?.Email ?? student?.Email;
            if (!string.IsNullOrWhiteSpace(email)) sb.AppendLine($"EMAIL:{email}");

            if (!string.IsNullOrWhiteSpace(person.Address))
                sb.AppendLine($"ADR;TYPE=HOME:;;{Escape(person.Address)};;;;");

            var title = employee?.Designation ?? teacher?.Designation;
            if (!string.IsNullOrWhiteSpace(title)) sb.AppendLine($"TITLE:{Escape(title)}");

            var details = new List<string>();
            if (employee != null)
            {
                Add(details, "ID", employee.EmployeeCode);
                Add(details, "Department", employee.Department);
                Add(details, "Working Area", employee.WorkingArea);
                Add(details, "Location", employee.Location);
                Add(details, "Emergency", employee.EmergencyContact);
            }
            else if (teacher != null)
            {
                Add(details, "ID", teacher.TeacherCode);
                Add(details, "Department", teacher.Department);
                Add(details, "Working Area", teacher.WorkingArea);
                Add(details, "Location", teacher.Location);
                Add(details, "Emergency", teacher.EmergencyContact);
            }
            else if (student != null)
            {
                Add(details, "Roll", student.RollNumber);
                Add(details, "Class", student.Class?.Name);
                Add(details, "Section", student.Section?.Name);
                Add(details, "Guardian", student.GuardianName);
            }

            Add(details, "NID/BC", person.IdNumber);
            Add(details, "Blood", employee?.BloodGroup ?? teacher?.BloodGroup ?? student?.BloodGroup);
            Add(details, "Issued", FmtDate(person.IssuedOn));
            Add(details, "Valid", FmtDate(person.ValidUntil));

            if (details.Count > 0)
                sb.AppendLine($"NOTE:{Escape(string.Join(" | ", details))}");

            sb.AppendLine("END:VCARD");
            return sb.ToString();
        }

        private static void Add(List<string> list, string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value)) list.Add($"{label}: {value}");
        }

        private static string Escape(string value) =>
            value.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");

        private void EnsureFontsRegistered()
        {
            lock (FontLock)
            {
                if (_fontsRegistered) return;

                var fontDir = Path.Combine(_environment.WebRootPath, "fonts", "Lato");
                foreach (var file in new[] { "Lato-Regular.ttf", "Lato-Bold.ttf" })
                {
                    var path = Path.Combine(fontDir, file);
                    if (!File.Exists(path)) continue;

                    using var stream = File.OpenRead(path);
                    FontManager.RegisterFont(stream);
                }

                _fontsRegistered = true;
            }
        }
    }
}
