using AdminToolsConsoleApp.Models;
using AdminToolsConsoleApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdminToolsConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Setup dependency injection
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
            });
            services.AddScoped<AlertEmailService>();

            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var alertEmailService = serviceProvider.GetRequiredService<AlertEmailService>();

            try
            {
                logger.LogInformation("StackOverflow Admin Tools Console App started");
                
                await ShowMainMenuAsync(alertEmailService, logger);
            }
            catch (InvalidOperationException ex)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                        GREŠKA                                ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine("Aplikacija ne može da se poveže na Azure Storage.");
                Console.WriteLine();
                Console.WriteLine("RAZLOG: Azurite (Azure Storage Emulator) nije pokrenut.");
                Console.WriteLine();
                Console.WriteLine("REŠENJE:");
                Console.WriteLine("1. Otvorite novi terminal/command prompt");
                Console.WriteLine("2. Pokrenite sledeću komandu:");
                Console.WriteLine("   azurite --silent --location c:\\azurite --debug c:\\azurite\\debug.log");
                Console.WriteLine("3. Pokrenite ovu aplikaciju ponovo");
                Console.WriteLine();
                Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
                Console.ReadKey();
                logger.LogError(ex, "Failed to start application - Azurite not running");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception in main application");
                Console.WriteLine($"\nNeočekivana greška: {ex.Message}");
                Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
                Console.ReadKey();
            }
            finally
            {
                logger.LogInformation("Application ended");
            }
        }

        static async Task ShowMainMenuAsync(AlertEmailService alertEmailService, ILogger logger)
        {
            bool exit = false;
            
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                StackOverflow Admin Tools                     ║");
                Console.WriteLine("║                Alert Email Management                        ║");
                Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
                Console.WriteLine("║  1. Prikaži sve email adrese                                 ║");
                Console.WriteLine("║  2. Dodaj novu email adresu                                  ║");
                Console.WriteLine("║  3. Obriši email adresu                                      ║");
                Console.WriteLine("║  4. Aktiviraj/Deaktiviraj email adresu                      ║");
                Console.WriteLine("║  5. Izlaz                                                    ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
                Console.Write("\nIzaberite opciju (1-5): ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await ShowAllEmailsAsync(alertEmailService);
                            break;
                        case "2":
                            await AddNewEmailAsync(alertEmailService);
                            break;
                        case "3":
                            await DeleteEmailAsync(alertEmailService);
                            break;
                        case "4":
                            await ToggleEmailStatusAsync(alertEmailService);
                            break;
                        case "5":
                            exit = true;
                            Console.WriteLine("\nIzlazim iz aplikacije...");
                            break;
                        default:
                            Console.WriteLine("\nNeispravna opcija! Pritisnite bilo koji taster za nastavak...");
                            Console.ReadKey();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing menu option: {Choice}", choice);
                    Console.WriteLine($"\nGreška: {ex.Message}");
                    Console.WriteLine("Pritisnite bilo koji taster za nastavak...");
                    Console.ReadKey();
                }
            }
        }

        static async Task ShowAllEmailsAsync(AlertEmailService alertEmailService)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("                    SVI EMAIL ADRESE");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");

            var emails = await alertEmailService.GetAllAlertEmailsAsync();

            if (!emails.Any())
            {
                Console.WriteLine("Nema registrovanih email adresa.");
            }
            else
            {
                Console.WriteLine($"{"Br.",-3} {"Email",-35} {"Ime",-20} {"Status",-10}");
                Console.WriteLine(new string('-', 70));

                for (int i = 0; i < emails.Count; i++)
                {
                    var email = emails[i];
                    var status = email.IsActive ? "Aktivan" : "Neaktivan";
                    Console.WriteLine($"{(i + 1),-3} {email.Email,-35} {email.Name,-20} {status,-10}");
                }

                Console.WriteLine(new string('-', 70));
                Console.WriteLine($"Ukupno: {emails.Count} email adresa");
            }

            Console.WriteLine("\nPritisnite bilo koji taster za povratak na glavni meni...");
            Console.ReadKey();
        }

        static async Task AddNewEmailAsync(AlertEmailService alertEmailService)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("                   DODAVANJE NOVOG EMAIL-a");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");

            Console.Write("Unesite email adresu: ");
            var email = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Email adresa ne može biti prazna!");
                Console.WriteLine("Pritisnite bilo koji taster za povratak...");
                Console.ReadKey();
                return;
            }

            Console.Write("Unesite ime vlasnika: ");
            var name = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Ime ne može biti prazno!");
                Console.WriteLine("Pritisnite bilo koji taster za povratak...");
                Console.ReadKey();
                return;
            }

            Console.Write($"\nDa li želite da dodate email '{email}' za '{name}'? (y/N): ");
            var confirm = Console.ReadLine()?.Trim().ToLower();

            if (confirm == "y" || confirm == "yes")
            {
                var success = await alertEmailService.AddAlertEmailAsync(email, name);
                
                if (success)
                {
                    Console.WriteLine($"\n✓ Email adresa '{email}' je uspešno dodana!");
                }
                else
                {
                    Console.WriteLine($"\n✗ Greška pri dodavanju email adrese. Email možda već postoji ili format nije ispravan.");
                }
            }
            else
            {
                Console.WriteLine("\nOperacija je otkazana.");
            }

            Console.WriteLine("Pritisnite bilo koji taster za povratak...");
            Console.ReadKey();
        }

        static async Task DeleteEmailAsync(AlertEmailService alertEmailService)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("                    BRISANJE EMAIL ADRESE");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");

            var emails = await alertEmailService.GetAllAlertEmailsAsync();

            if (!emails.Any())
            {
                Console.WriteLine("Nema registrovanih email adresa za brisanje.");
                Console.WriteLine("Pritisnite bilo koji taster za povratak...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Dostupne email adrese:");
            Console.WriteLine($"{"Br.",-3} {"Email",-35} {"Ime",-20} {"Status",-10}");
            Console.WriteLine(new string('-', 70));

            for (int i = 0; i < emails.Count; i++)
            {
                var email = emails[i];
                var status = email.IsActive ? "Aktivan" : "Neaktivan";
                Console.WriteLine($"{(i + 1),-3} {email.Email,-35} {email.Name,-20} {status,-10}");
            }

            Console.Write($"\nUnesite broj email adrese za brisanje (1-{emails.Count}) ili 0 za povratak: ");
            var input = Console.ReadLine()?.Trim();

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= emails.Count)
            {
                var selectedEmail = emails[choice - 1];
                Console.Write($"\nDa li STVARNO želite da obrišete '{selectedEmail.Email}'? (y/N): ");
                var confirm = Console.ReadLine()?.Trim().ToLower();

                if (confirm == "y" || confirm == "yes")
                {
                    var success = await alertEmailService.DeleteAlertEmailAsync(selectedEmail.Email);
                    
                    if (success)
                    {
                        Console.WriteLine($"\n✓ Email adresa '{selectedEmail.Email}' je uspešno obrisana!");
                    }
                    else
                    {
                        Console.WriteLine($"\n✗ Greška pri brisanju email adrese.");
                    }
                }
                else
                {
                    Console.WriteLine("\nOperacija je otkazana.");
                }
            }
            else if (choice != 0)
            {
                Console.WriteLine("\nNeispravna opcija!");
            }

            if (choice != 0)
            {
                Console.WriteLine("Pritisnite bilo koji taster za povratak...");
                Console.ReadKey();
            }
        }

        static async Task ToggleEmailStatusAsync(AlertEmailService alertEmailService)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("              AKTIVIRANJE/DEAKTIVIRANJE EMAIL ADRESE");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");

            var emails = await alertEmailService.GetAllAlertEmailsAsync();

            if (!emails.Any())
            {
                Console.WriteLine("Nema registrovanih email adresa.");
                Console.WriteLine("Pritisnite bilo koji taster za povratak...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Dostupne email adrese:");
            Console.WriteLine($"{"Br.",-3} {"Email",-35} {"Ime",-20} {"Status",-10}");
            Console.WriteLine(new string('-', 70));

            for (int i = 0; i < emails.Count; i++)
            {
                var email = emails[i];
                var status = email.IsActive ? "Aktivan" : "Neaktivan";
                Console.WriteLine($"{(i + 1),-3} {email.Email,-35} {email.Name,-20} {status,-10}");
            }

            Console.Write($"\nUnesite broj email adrese za promenu statusa (1-{emails.Count}) ili 0 za povratak: ");
            var input = Console.ReadLine()?.Trim();

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= emails.Count)
            {
                var selectedEmail = emails[choice - 1];
                var currentStatus = selectedEmail.IsActive ? "aktivna" : "neaktivna";
                var newStatus = selectedEmail.IsActive ? "deaktivirati" : "aktivirati";
                
                Console.Write($"\nEmail adresa '{selectedEmail.Email}' je trenutno {currentStatus}.");
                Console.Write($"\nDa li želite da je {newStatus}? (y/N): ");
                var confirm = Console.ReadLine()?.Trim().ToLower();

                if (confirm == "y" || confirm == "yes")
                {
                    var success = await alertEmailService.ToggleAlertEmailStatusAsync(selectedEmail.Email);
                    
                    if (success)
                    {
                        var finalStatus = selectedEmail.IsActive ? "deaktivirana" : "aktivirana";
                        Console.WriteLine($"\n✓ Email adresa '{selectedEmail.Email}' je uspešno {finalStatus}!");
                    }
                    else
                    {
                        Console.WriteLine($"\n✗ Greška pri promeni statusa email adrese.");
                    }
                }
                else
                {
                    Console.WriteLine("\nOperacija je otkazana.");
                }
            }
            else if (choice != 0)
            {
                Console.WriteLine("\nNeispravna opcija!");
            }

            if (choice != 0)
            {
                Console.WriteLine("Pritisnite bilo koji taster za povratak...");
                Console.ReadKey();
            }
        }
    }
}
