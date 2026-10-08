# JoScraper

JoScraper is an ASP.NET Core backend project for scraping and managing products from online stores.

The main idea behind the project is to automate the process of collecting product data, storing it in a database, organizing products into categories, and making the data faster to access through caching.

The project also uses AI for product categorization. It first tries to match a product with an existing category using embeddings and semantic similarity. If it cannot find a good match, it uses an LLM to generate a suitable category.

## Features

* Web scraping using Playwright and HtmlAgilityPack
* Product and store management
* Duplicate product detection
* Automatic product categorization
* Embedding-based semantic similarity
* LLM integration for creating new categories
* Redis caching
* Hangfire background and scheduled scraping jobs
* Serilog logging
* Entity Framework Core
* SQL Server
* Database indexing
* Repository and service-based architecture
* Support for adding more stores through the scraper abstraction

## Tech Stack

### Backend

* C#
* .NET / ASP.NET Core
* Entity Framework Core
* REST API

### Database

* Microsoft SQL Server
* Entity Framework Core
* Database indexes

### Scraping

* Microsoft Playwright
* HtmlAgilityPack

### Caching

* Redis

### Background Jobs

* Hangfire
* Hangfire SQL Server storage

### Logging

* Serilog

### AI

* Jina AI Embeddings
* Gemini / LLM
* Cosine similarity
* Semantic search

---

## How the Scraping Works

Each store has its own scraper implementation. This makes it possible to add another store without having to change the rest of the application.

Playwright is used for websites that rely on JavaScript to load their content. After the page is loaded, HtmlAgilityPack is used to parse the HTML and extract the required information.

The scraper currently collects information such as:

* Product name
* Product URL
* Product price
* Store information

The scraper also handles different price elements when a store has special prices, previous prices, or regular prices.

Before saving a product, the system checks whether it already exists. This is important because scraping the same store multiple times should not create duplicate records.

---

## Product and Store Structure

Products and stores are separated in the database.

An `Item` represents the product itself, while a `StoreItem` represents that product as it appears in a specific store.

This allows the same product to exist across multiple stores without having to create completely separate product records.

This structure also makes it easier to work with store-specific information such as URLs and prices.

---

## Duplicate Detection

Duplicate detection happens at multiple levels.

Product URLs are used to identify products from the same store. The system also checks for duplicates within the same scraping operation before saving the data.

For store items, the application considers the product, store, and price when deciding whether a record should be inserted.

This means that if the same product is scraped again from the same store at the same price, another record is not created.

If the price changes, the system can treat it as a new store-item price instead of considering it the exact same record.

---

# Product Categorization

One of the main parts of JoScraper is the automatic product categorization system.

Instead of sending every product directly to an LLM, the system first tries to match the product with categories that already exist in the database.

For example, if the database already contains:

* Keyboards
* Computer Mice
* Monitors
* Headphones

and a new product called:

```text
Wireless Gaming Mechanical Keyboard
```

is scraped, the system generates an embedding for the product name and compares it with the embeddings of the existing categories.

If one of the categories has a high enough similarity score, the product is assigned to that category.

This avoids unnecessary LLM requests and makes the categorization process more efficient.

---

## Embeddings

Embeddings are used to convert product and category text into numerical vectors.

For example:

```text
Wireless Gaming Mechanical Keyboard
```

is converted into a vector representation.

The same is done for the categories stored in the database.

The system then compares the vectors using cosine similarity.

The category with the highest similarity is selected if its score is above the configured threshold.

The embedding functionality is abstracted behind a service, which makes it possible to change the embedding provider without having to rewrite the categorization logic.

The current implementation uses Jina AI embeddings.

---

## LLM Categorization

If none of the existing categories is similar enough to the product, the system falls back to an LLM.

The LLM is asked to generate a general category for the product rather than simply returning the full product name.

For example:

```text
Product:
Logitech G Pro X Mechanical Gaming Keyboard

LLM:
Keyboards
```

After receiving the category, the application checks whether a category with the same name already exists.

If it exists, the existing category is reused.

If it does not exist, a new category is created and its embedding is generated and stored.

This gives the system a combination of semantic matching and LLM-based categorization instead of relying completely on an LLM for every product.

---

# Redis Caching

Redis is used to cache data that is requested frequently.

For example, when retrieving products from a specific store, the application first checks Redis.

If the data is already cached, it can be returned without querying SQL Server again.

If it is not cached, the application gets the data from the database and then stores it in Redis for future requests.

This helps reduce database load and improve API response times.

Cache entries are also invalidated when the underlying data changes so that outdated information is not continuously returned.

---

# Hangfire

Scraping can take a significant amount of time, so it should not run directly inside a normal HTTP request.

JoScraper uses Hangfire to run scraping operations as background jobs.

This allows the API to start a scraping job and return without keeping the HTTP request open until the entire scraping process finishes.

Hangfire is also used for scheduled scraping jobs.

For example, a store can be configured to run its scraper periodically instead of having to manually trigger it every time.

Hangfire uses SQL Server for job storage, so jobs can persist even if the application is restarted.

The Hangfire dashboard can be accessed through:

```text
/hangfire
```

It can be used to view running, completed, scheduled, and failed jobs.

---

# Serilog Logging

JoScraper uses Serilog for application logging.

Logging is used throughout the application to keep track of important operations and make debugging easier.

Examples include:

* API requests
* Scraping operations
* Background jobs
* Errors and exceptions
* Database operations
* Important application events

The application uses structured logging so that values such as IDs and other properties can be stored separately and searched more easily.

---

# Database and Indexing

SQL Server is used as the main database, with Entity Framework Core handling database access.

The application follows a repository and service-based structure so that controllers are not directly responsible for database operations.

Database indexes are also used on fields that are frequently searched or filtered.

This becomes especially important when the scraper starts collecting thousands or millions of products.

Indexes help SQL Server find the required records without having to scan the entire table every time.

---

# Project Structure

The project is separated into different parts based on their responsibility.

Repository Pattern was used for this Project.

The main responsibilities are:

**Controllers**

Handle HTTP requests and responses.

**Services**

Contain the main business logic such as scraping, categorization, embeddings, and other application operations.

**Repositories**

Handle database operations through Entity Framework Core.

**Models**

Contain the database entities such as `Item`, `Store`, `StoreItem`, and `Category`.

**DTOs**

Contain objects used for transferring data between the API and the client.

---

# Requirements

To run the project locally, you will need:

* .NET SDK
* SQL Server
* Redis / Docker container that have redis server running on it
* Git
* A supported browser for Playwright - chromium as an example
* Jina AI API key (for embeddings)
* Gemini API key (for LLM categorization)

---

# Getting Started

Clone the repository:

```bash
git clone <repository-url>
cd JoScraper
```

Restore the project dependencies:

```bash
dotnet restore
```

Build the project:

```bash
dotnet build
```

Configure your database connection and required API keys.

If Entity Framework migrations are included, update the database:

```bash
dotnet ef database update
```

Install the Playwright browsers if they are not already installed:

```bash
pwsh bin/Debug/net*/playwright.ps1 install
```

Then run the application:

```bash
dotnet run
```

The API URL will be displayed in the terminal when the application starts.

If Hangfire is enabled, the dashboard will be available at:

```text
/hangfire
```

---

# Typical Workflow

A normal scraping operation roughly follows this process:

1. A scraping job is started manually or through Hangfire.
2. Playwright opens the target store.
3. The scraper navigates through the required pages.
4. Product information is extracted from the HTML.
5. Duplicate products are detected.
6. New products are stored in SQL Server.
7. Products are sent to the categorization service.
8. An embedding is generated for the product.
9. The product is compared with existing categories.
10. If a suitable category is found, it is assigned to the product.
11. If no suitable category exists, the LLM generates one.
12. The system checks whether that category already exists.
13. The existing category is reused or a new category is created.
14. Relevant cached data is invalidated or updated.
15. The operation is logged using Serilog.

---

# Why I Built It

The project started as a web scraping project, but I wanted to go beyond simply scraping websites and putting the data into a database.

The main goal was to combine scraping with different backend technologies and AI techniques that are useful in a real application.

This includes handling duplicate data, optimizing database queries, caching frequently accessed data, running long-running tasks in the background, and using embeddings and LLMs to automatically organize the scraped products.

---

# Future Improvements

Some things I would like to add or improve in the future include:

* Adding more online stores
* Product price history
* Better scraper error handling and retries
* Automated tests
* Cloud deployment
* Monitoring and metrics
* A frontend for browsing and searching the scraped products

---

# Author

**Yahya Jad**

ASP.NET Core / Full Stack Developer

GitHub: `yahyajad04`

LinkedIn: `yahya-jad-9227a124`
