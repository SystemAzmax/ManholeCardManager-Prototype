# Manhole Card Manager

English | [日本語](README.ja.md)

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![Windows App SDK](https://img.shields.io/badge/Windows%20App%20SDK-1.8-0078D4?logo=windows)](https://github.com/microsoft/WindowsAppSDK)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A Windows desktop application for managing your manhole card collection.

## 📖 Overview

Manhole Card Manager is an application designed to help you organize and track your collection of manhole cards distributed across Japan. It features two main views - Card Collection and Distribution Locations - to efficiently manage your collection.

### Key Features

- ✅ **Card Collection Management**: View and manage your acquired manhole cards
- 📍 **Distribution Location List**: Browse distribution locations organized by prefecture and municipality
- 🎯 **Acquisition Tracking**: Record acquisition dates and status for each card
- 🌐 **Multi-language Support**: Switch between Japanese and English UI
- 🗄️ **Local Database**: Lightweight and fast data management using SQLite
- 🖼️ **Image Caching**: Efficient card image display

## 🚀 Getting Started

### System Requirements

- **OS**: Windows 10 (19041) or later
- **Runtime**: .NET 8.0
- **Architecture**: x64

### Installation

1. Clone the repository:
```bash
git clone https://github.com/SystemAzmax/ManholeCardManager-Prototype.git
cd ManholeCardManager
```

2. Build the solution:
```bash
dotnet build
```

3. Run the application:
```bash
cd ManholeCardManager\ManholeCardManager
dotnet run
```

## 🏗️ Project Structure

This repository consists of three projects:

### 1. ManholeCardManager (Main Application)
A desktop application built with WinUI 3.

**Main Components:**
- `ViewModels/`: MVVM pattern view models
  - `MainWindowViewModel.cs`: Main window logic
  - `CardCollectionViewModel.cs`: Card collection view management
  - `DistributionLocationViewModel.cs`: Distribution location view management
- `Services/`: Application services
  - `DatabaseService.cs`: SQLite database operations
  - `LocalizationService.cs`: Multi-language support
  - `ImageCacheService.cs`: Image cache management
- `Models/`: Data models
  - `ManholeCard.cs`: Manhole card information
  - `CardLocation.cs`: Card distribution locations
  - `AcquisitionHistory.cs`: Acquisition history

### 2. ManholeCardDataScraper (Data Scraper)
A tool to fetch distribution location data from the Gesuido Kouhou Platform (GK-P).

See [ManholeCardDataScraper/README.md](ManholeCardDataScraper/README.md) for details.

### 3. ManholeCardManager.Tests (Test Project)
Unit test project.

## 🎮 Usage

### First Launch

1. When you launch the application for the first time, a local database will be created automatically
2. It's recommended to run the data scraper to sync distribution location data

### Managing Cards

1. **Card Collection View**: Display list of acquired cards
   - Shows card images, locations, and acquisition dates
   - Filter by series number

2. **Distribution Location View**: Display locations by region
   - Filter by prefecture and municipality
   - View card information available at each location
   - Display location addresses and distribution methods

### Language Switching

You can switch between Japanese and English from the settings.

## 🔧 Technology Stack

- **Framework**: .NET 8.0
- **UI**: WinUI 3 (Windows App SDK 1.8)
- **Database**: SQLite (Microsoft.Data.Sqlite)
- **Architecture**: MVVM Pattern
- **Logging**: Microsoft.Extensions.Logging

## 📊 Database Schema

The application uses the following main tables:

- `ManholeCards`: Card information
- `CardLocations`: Distribution location information
- `ManholeLocations`: Manhole installation locations
- `AcquisitionHistory`: Card acquisition history

## 🛠️ Development

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Debug

Open the solution in Visual Studio 2022 and press F5 to start debugging.

## 📝 License

This project is licensed under the [MIT License](LICENSE).

## 🤝 Contributing

Issues and Pull Requests are welcome!

1. Fork this repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Create a Pull Request

## 📮 Contact

For questions or suggestions about the project, please use [Issues](https://github.com/SystemAzmax/ManholeCardManager-Prototype/issues).

## 🙏 Acknowledgments

- Distribution location data is obtained from the [Gesuido Kouhou Platform (GK-P)](https://www.gk-p.jp/)

---

**Note**: This is a prototype under active development. Features and specifications are subject to change.
