using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using IDCardBD.Web.Models;

namespace IDCardBD.Web.Services
{
    public class PdfService : IPdfService
    {
        private readonly IWebHostEnvironment _environment;

        public PdfService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public byte[] GenerateIdCard(IdentityBase person, CardTemplate template)
        {
            // ID Card Dimensions: 85.6mm x 53.98mm
            // We adding a small bleed or just exact size. 
            // QuestPDF uses Points by default. 1 inch = 72 points.
            // 85.6mm = 3.37 inch = ~242.64 points
            // 53.98mm = 2.125 inch = ~153 points

             var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(new PageSize(242.64f, 153f));
                    page.Margin(0);
                    
                    page.Content().Layers(layers =>
                    {
                        // Background Layer (Front)
                        string frontBgPath = Path.Combine(_environment.WebRootPath, template.FrontBgPath.TrimStart('/').Replace("/", "\\"));
                        if (File.Exists(frontBgPath))
                        {
                            layers.Layer().Image(frontBgPath).FitArea();
                        }
                        else
                        {
                            layers.Layer().Background(Colors.White);
                        }

                        // Content Layer
                        layers.PrimaryLayer().Padding(10).Column(column =>
                        {
                            column.Item().Text(person.FullName).FontSize(12).Bold().FontColor(Colors.Black);
                            
                            if (person is Student student)
                            {
                                column.Item().Text($"Roll: {student.RollNumber}");
                                column.Item().Text($"Grade: {student.Grade}");
                            }
                            else if (person is Employee employee)
                            {
                                column.Item().Text($"ID: {employee.EmployeeCode}");
                                column.Item().Text($"{employee.Designation}");
                            }

                            // Photo placeholder if exists
                            // Note: Implementation specific layout needed
                        });
                    });
                });

                container.Page(page =>
                {
                    page.Size(new PageSize(242.64f, 153f));
                    page.Margin(0);

                    page.Content().Layers(layers =>
                    {
                        // Background Layer (Back)
                        string backBgPath = Path.Combine(_environment.WebRootPath, template.BackBgPath.TrimStart('/').Replace("/", "\\"));
                        if (File.Exists(backBgPath))
                        {
                            layers.Layer().Image(backBgPath).FitArea();
                        }
                        
                         // Content Layer (Back)
                        layers.PrimaryLayer().Padding(10).Column(column =>
                        {
                             // Static content or back side details
                             column.Item().AlignBottom().AlignRight().Text("Authorized Signature").FontSize(8);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
