using AcousticVision.Data;
using AcousticVision.Services;
using AcousticVision.ViewModels;
using AcousticVision.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AcousticVision;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=acousticvision.db"));

        serviceCollection.AddSingleton<MainWindowViewModel>();
        serviceCollection.AddScoped<MaterialService>();
        serviceCollection.AddScoped<TextureService>();
        serviceCollection.AddScoped<RoomModelService>();
        serviceCollection.AddScoped<RoomSurfaceService>();
        serviceCollection.AddScoped<SoundSourceService>();
        serviceCollection.AddScoped<SoundReceiverService>();
        serviceCollection.AddScoped<TestModelService>();
        serviceCollection.AddScoped<AnalysisService>();

        serviceCollection.AddTransient<MaterialsViewModel>();
        serviceCollection.AddTransient<TexturesViewModel>();
        serviceCollection.AddTransient<RoomsViewModel>();
        serviceCollection.AddTransient<RoomSurfacesViewModel>();
        serviceCollection.AddTransient<SourcesViewModel>();
        serviceCollection.AddTransient<ReceiversViewModel>();
        serviceCollection.AddTransient<TestModelsViewModel>();
        serviceCollection.AddTransient<AnalysisViewModel>();

        Services = serviceCollection.BuildServiceProvider();

        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            Seed(db);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void Seed(AppDbContext db)
    {
        if (!db.Materials.Any())
        {
            db.Materials.AddRange(
                new Models.Material { Name = "Минвата", NoiseCancelation = 0.75 },
                new Models.Material { Name = "Бетон", NoiseCancelation = 0.05 },
                new Models.Material { Name = "Ковролин", NoiseCancelation = 0.40 }
            );
            db.SaveChanges();
        }

        if (!db.Textures.Any())
        {
            db.Textures.AddRange(
                new Models.Texture { Name = "Гладкая", NoiseCancelation = 0.08 },
                new Models.Texture { Name = "Шероховатая", NoiseCancelation = 0.22 },
                new Models.Texture { Name = "Пористая", NoiseCancelation = 0.45 }
            );
            db.SaveChanges();
        }

        if (!db.SoundSources.Any())
        {
            db.SoundSources.AddRange(
                new Models.SoundSource
                {
                    Name = "Речь, мужской голос",
                    Volume = 62,
                    Article = 0.80,
                    Properties = "type=speech"
                },
                new Models.SoundSource
                {
                    Name = "Тестовый тон 1 кГц",
                    Volume = 70,
                    Article = null,
                    Properties = "type=test; freq=1000Hz"
                }
            );
            db.SaveChanges();
        }

        if (!db.SoundReceivers.Any())
        {
            db.SoundReceivers.AddRange(
                new Models.SoundReceiver
                {
                    Name = "Слушатель 1",
                    Properties = "type=listener; profile=avg"
                },
                new Models.SoundReceiver
                {
                    Name = "Микрофон A",
                    Properties = "type=mic; sensitivity=1.0"
                }
            );
            db.SaveChanges();
        }

        if (!db.RoomModels.Any())
        {
            db.RoomModels.AddRange(
                new Models.RoomModel { Name = "Аудитория 312", Length = 10.0, Width = 7.0, Height = 3.2 },
                new Models.RoomModel { Name = "Кабинет 101", Length = 8.0, Width = 5.5, Height = 3.0 }
            );
            db.SaveChanges();
        }

        if (!db.RoomSurfaces.Any() && db.RoomModels.Any() && db.Materials.Any() && db.Textures.Any())
        {
            var room = db.RoomModels.OrderBy(x => x.Id).First();
            var materials = db.Materials.OrderBy(x => x.Id).ToList();
            var textures = db.Textures.OrderBy(x => x.Id).ToList();

            string[] positions = { "floor", "ceiling", "north", "south", "east", "west" };
            for (int i = 0; i < positions.Length; i++)
            {
                db.RoomSurfaces.Add(new Models.RoomSurface
                {
                    RoomId = room.Id,
                    Position = positions[i],
                    MaterialId = materials[i % materials.Count].Id,
                    TextureId = textures[i % textures.Count].Id
                });
            }
            db.SaveChanges();
        }

        if (!db.TestModels.Any() && db.RoomModels.Any() && db.SoundSources.Any() && db.SoundReceivers.Any())
        {
            db.TestModels.Add(new Models.TestModel
            {
                RoomId = db.RoomModels.OrderBy(x => x.Id).First().Id,
                SourceId = db.SoundSources.OrderBy(x => x.Id).First().Id,
                ReceiverId = db.SoundReceivers.OrderBy(x => x.Id).First().Id,
                SourceLocation = "(1.0; 1.0; 1.5)",
                ReceiverLocation = "(4.0; 3.0; 1.2)"
            });
            db.SaveChanges();
        }
    }
}
