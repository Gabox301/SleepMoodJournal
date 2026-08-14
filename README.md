# Diario de Sueño y Ánimo

App de escritorio para registrar diariamente horas y calidad de sueño, estado
de ánimo, y visualizar tendencias a lo largo del tiempo.

## Stack técnico

| Capa         | Tecnología                                            |
| ------------ | ----------------------------------------------------- |
| Framework    | .NET 10                                               |
| UI           | Avalonia UI 11 (multiplataforma: Windows/macOS/Linux) |
| Patrón       | MVVM con CommunityToolkit.Mvvm                        |
| Persistencia | SQLite vía EF Core                                    |
| Gráficos     | LiveCharts2 (LiveChartsCore.SkiaSharpView.Avalonia)   |

## Estructura del proyecto

```
SleepMoodJournal/
├── Models/          # Entidades del dominio (DailyEntry)
├── Data/            # DbContext y acceso a datos (SQLite)
├── ViewModels/       # Lógica de presentación (MVVM)
├── Views/            # XAML (Avalonia)
├── docs/             # Esta documentación
├── App.axaml(.cs)    # Bootstrap de la aplicación
└── Program.cs         # Entry point
```

La base SQLite se guarda en `%LocalAppData%/SleepMoodJournal/journal.db`
(o el equivalente en macOS/Linux), no en la carpeta de instalación.

## Funcionalidades core (v1)

- **Registro diario**
  - Horas de sueño (0-12, con decimales).
  - Calidad del sueño (escala 1-5).
  - Estado de ánimo (escala 1-5).
  - Notas libres opcionales.
  - Un solo registro por día (índice único por fecha); si ya existe, se edita.
- **Vista de tendencias**
  - Gráfico de línea de horas de sueño en el tiempo.
  - Gráfico de línea de estado de ánimo en el tiempo.
  - Selector de rango (7/30/90/365 días vía `DaysToShow`).
  - Promedios del período.
  - Correlación simple: ánimo promedio en días de "buen sueño" (≥7h) vs. el resto.

## Features para sumar después

### Datos y contexto

- **Tags de contexto** (ejercicio, cafeína, alcohol, estrés, pantallas antes
  de dormir) para cruzar con ánimo/sueño — hoy solo existe el campo `Notes`
  libre, pero tags estructurados permiten agregaciones ("¿cómo duermo los
  días que hago ejercicio?").
- **Hora de dormir / hora de despertar** en vez de (o además de) horas totales,
  para detectar patrones de horario, no solo de duración.
- **Registro retroactivo simplificado** — hoy se puede elegir cualquier fecha
  en el DatePicker, pero falta una vista tipo calendario para completar
  huecos de días pasados de un vistazo.

### Visualización

- **Vista de calendario** tipo heatmap (estilo GitHub contributions) coloreado
  por ánimo o calidad de sueño.
- **Gráfico combinado** sueño + ánimo superpuestos en un solo chart para ver
  la correlación visualmente, no solo en texto.
- **Estadísticas más ricas**: mejor/peor semana, rachas (streaks) de buen
  sueño, desviación estándar.

### Productividad

- **Recordatorio diario** (notificación local del SO) para no olvidarse de
  registrar — requiere integrar notificaciones nativas por plataforma.
- **Exportar a CSV/JSON** para análisis externo o backup.
- **Importar backup** — restaurar desde un CSV/JSON exportado previamente.

### Calidad de vida

- **Modo oscuro** — Avalonia FluentTheme lo soporta casi out-of-the-box,
  falta exponer el toggle en la UI.
- **Atajos de teclado** para registrar rápido sin usar el mouse.
- **Multi-perfil** — si más de una persona en la misma compu quiere llevar
  su propio diario.

## Cómo correr el proyecto (Desarrollo)

```bash
dotnet restore
dotnet run
```

Requiere el SDK de .NET 10 instalado. La primera vez que corre, crea la
base SQLite automáticamente (`EnsureCreated`, sin migraciones formales).
Si el modelo de datos crece o cambia, conviene migrar a
`dotnet ef migrations add ...` en vez de `EnsureCreated`.

## Generar ejecutable single-file

Para probar o distribuir la app como un único `.exe` **auto-contenido** para
Windows (no requiere tener el SDK/runtime de .NET instalado en la máquina destino):

```bash
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o publish/win-x64/single-file
```

El ejecutable queda en `publish/win-x64/single-file/SleepMoodJournal.exe`.

> **Nota de tamaño:** pesa ~100 MB porque un single-file auto-contenido empaqueta
> el runtime de .NET y las librerías nativas (Avalonia + Skia).
