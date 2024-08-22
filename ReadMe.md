# Sleeper Keeper API

This Azure Function retrieves data from the fantasy football league Sleeper API, specifically focusing on keeper player information.

## Features

- **GetKeepers Function**: Retrieves and processes keeper player data from the Sleeper API.
- **Asynchronous Processing**: Utilizes async functions for efficient data handling.
- **JSON Handling**: Fetches JSON data and converts it into a list of `Player` objects.

## Prerequisites

- .NET 6.0 SDK
- Azure Functions Core Tools (for local development)
- Git

## Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/eric22f/Sleeper.git
   ```
2. **Navigate to the project directory**:
   ```bash
   cd Sleeper
   ```
3. **Install dependencies**:
   ```bash
   dotnet restore
   ```
4. **Run the function locally**:
   ```bash
   func start
   ```

## Usage

Deploy the function to Azure and set up any required environment variables or configurations in the Azure portal. The `GetKeepers` function will automatically run according to the configured schedule or can be triggered manually.

## Contributing

Feel free to fork the repository and submit pull requests. Contributions are welcome!

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

This should give a concise overview of your project, helping others understand its purpose and how to get started. You can adjust the details to better fit your specific needs or add any additional sections that might be useful.
