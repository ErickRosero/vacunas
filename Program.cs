using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CovidVacunacionApp
{
    public class Ciudadano
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public List<string> Vacunas { get; set; } = new List<string>();

        public Ciudadano(int id)
        {
            Id = id;
            Nombre = $"Ciudadano {id}";
        }
    }

    public class ProgramaVacunacion
    {
        private HashSet<Ciudadano> todosCiudadanos;
        private HashSet<Ciudadano> vacunadosPfizer;
        private HashSet<Ciudadano> vacunadosAstrazeneca;

        public ProgramaVacunacion()
        {
            todosCiudadanos = new HashSet<Ciudadano>();
            vacunadosPfizer = new HashSet<Ciudadano>();
            vacunadosAstrazeneca = new HashSet<Ciudadano>();
            
            GenerarDatosFicticios();
        }

        private void GenerarDatosFicticios()
        {
            // Generar 500 ciudadanos
            for (int i = 1; i <= 500; i++)
            {
                todosCiudadanos.Add(new Ciudadano(i));
            }

            // Generar aleatoriamente 75 vacunados con Pfizer
            var random = new Random();
            var ciudadanosDisponibles = todosCiudadanos.ToList();

            for (int i = 0; i < 75; i++)
            {
                int index = random.Next(ciudadanosDisponibles.Count);
                var ciudadano = ciudadanosDisponibles[index];
                ciudadano.Vacunas.Add("Pfizer");
                vacunadosPfizer.Add(ciudadano);
                ciudadanosDisponibles.RemoveAt(index);
            }

            // Generar aleatoriamente 75 vacunados con Astrazeneca
            ciudadanosDisponibles = todosCiudadanos.Except(vacunadosPfizer).ToList();
            for (int i = 0; i < 75; i++)
            {
                int index = random.Next(ciudadanosDisponibles.Count);
                var ciudadano = ciudadanosDisponibles[index];
                ciudadano.Vacunas.Add("Astrazeneca");
                vacunadosAstrazeneca.Add(ciudadano);
                ciudadanosDisponibles.RemoveAt(index);
            }

            // Asignar segunda dosis aleatoriamente
            foreach (var ciudadano in vacunadosPfizer.Concat(vacunadosAstrazeneca))
            {
                if (random.Next(2) == 0) // 50% de probabilidad de segunda dosis
                {
                    ciudadano.Vacunas.Add(ciudadano.Vacunas[0]);
                }
            }
        }

        public HashSet<Ciudadano> ObtenerNoVacunados()
        {
            return new HashSet<Ciudadano>(todosCiudadanos.Except(vacunadosPfizer.Union(vacunadosAstrazeneca)));
        }

        public HashSet<Ciudadano> ObtenerCompletamenteVacunados()
        {
            return new HashSet<Ciudadano>(
                todosCiudadanos.Where(c => c.Vacunas.Count >= 2)
            );
        }

        public HashSet<Ciudadano> ObtenerVacunadosSoloPfizer()
        {
            return new HashSet<Ciudadano>(vacunadosPfizer.Except(vacunadosAstrazeneca));
        }

        public HashSet<Ciudadano> ObtenerVacunadosSoloAstrazeneca()
        {
            return new HashSet<Ciudadano>(vacunadosAstrazeneca.Except(vacunadosPfizer));
        }

        public void GenerarReportePDF(string rutaArchivo)
        {
            using (FileStream fs = new FileStream(rutaArchivo, FileMode.Create))
            {
                Document documento = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(documento, fs);

                documento.Open();

                documento.Add(new Paragraph("Reporte de Vacunación COVID-19"));
                documento.Add(new Paragraph("-----------------------------"));
                documento.Add(new Paragraph("\n"));

                // No vacunados
                documento.Add(new Paragraph("Ciudadanos No Vacunados:"));
                foreach (var ciudadano in ObtenerNoVacunados())
                {
                    documento.Add(new Paragraph($"- {ciudadano.Nombre}"));
                }
                documento.Add(new Paragraph("\n"));

                // Completamente vacunados
                documento.Add(new Paragraph("Ciudadanos con Esquema Completo:"));
                foreach (var ciudadano in ObtenerCompletamenteVacunados())
                {
                    documento.Add(new Paragraph($"- {ciudadano.Nombre} ({string.Join(", ", ciudadano.Vacunas)})"));
                }
                documento.Add(new Paragraph("\n"));

                // Solo Pfizer
                documento.Add(new Paragraph("Ciudadanos Vacunados Solo con Pfizer:"));
                foreach (var ciudadano in ObtenerVacunadosSoloPfizer())
                {
                    documento.Add(new Paragraph($"- {ciudadano.Nombre}"));
                }
                documento.Add(new Paragraph("\n"));

                // Solo Astrazeneca
                documento.Add(new Paragraph("Ciudadanos Vacunados Solo con Astrazeneca:"));
                foreach (var ciudadano in ObtenerVacunadosSoloAstrazeneca())
                {
                    documento.Add(new Paragraph($"- {ciudadano.Nombre}"));
                }

                documento.Close();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando programa de vacunación...\n");
            
            var programa = new ProgramaVacunacion();

            Console.WriteLine("Generando reportes...\n");

            // Mostrar resultados en consola
            Console.WriteLine("Ciudadanos No Vacunados:");
            foreach (var ciudadano in programa.ObtenerNoVacunados())
            {
                Console.WriteLine($"- {ciudadano.Nombre}");
            }

            Console.WriteLine("\nCiudadanos con Esquema Completo:");
            foreach (var ciudadano in programa.ObtenerCompletamenteVacunados())
            {
                Console.WriteLine($"- {ciudadano.Nombre}");
            }

            Console.WriteLine("\nCiudadanos Vacunados Solo con Pfizer:");
            foreach (var ciudadano in programa.ObtenerVacunadosSoloPfizer())
            {
                Console.WriteLine($"- {ciudadano.Nombre}");
            }

            Console.WriteLine("\nCiudadanos Vacunados Solo con Astrazeneca:");
            foreach (var ciudadano in programa.ObtenerVacunadosSoloAstrazeneca())
            {
                Console.WriteLine($"- {ciudadano.Nombre}");
            }

            // Generar reporte PDF
            try
            {
                programa.GenerarReportePDF("ReporteVacunacion.pdf");
                Console.WriteLine("\nReporte PDF generado exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError al generar el PDF: {ex.Message}");
            }

            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}