# Hotel Booking Platform API 
Welcome to My Hotel Booking Platform API, a comprehensive solution designed to streamline hotel management and enhance guest experiences. This API efficiently manages bookings, hotel and city data, and various guest services.
The live API is accessible at [https://hotelbookingblatform-ajewdsbpbwetccg5.canadacentral-01.azurewebsites.net/](https://hotelbookingblatform-ajewdsbpbwetccg5.canadacentral-01.azurewebsites.net/swagger/index.html). Feel free to explore its functionalities in a real-world environment.

This project was developed during my backend internship at Foothill Technology Solutions, By building it, I gained hands-on experience with **RESTful API development**, leveraging **Entity Framework (EF) as an ORM** to interact with the database efficiently. Writing **LINQ queries** improved my ability to work with data effectively, while implementing **unit testing** enhanced my skills in writing reliable and maintainable code.  

To ensure clean and scalable architecture, I applied **clean code principles and design patterns**, using **AutoMapper** for seamless object-to-object mapping and **FluentValidation** for robust input validation. The project follows a **Clean Architecture (Presentation Layer (Web API), Application Layer (Use Cases), Domain Layer (Core Business Models), Infrastructure Layer (Data & External Services).)**, a structured approach that enhances maintainability by clearly separating concerns—allowing independent scaling, testing, and modification of each layer without affecting the others.  

This structured approach reinforced best practices in software architecture and enhanced my ability to design and develop enterprise-grade applications.

---

## Documentation
For a complete overview of all available endpoints, it is enriched with usage examples and expected error responses. refer to:
### Swagger Documentation

- **Hotel Booking Platform API v1**
  - [OAS3 Documentation](https://app.swaggerhub.com/apis-docs/raghadhanon-609/hotel-booking_blatform_api/v1#/)

### Postman Documentation

- **Hotel Booking Platform API v1**
  - [Documentation](https://documenter.getpostman.com/view/27912762/2sAYX3q3Kx)

---

## ⭐ Key Features

### Authentication Management:
- **Admin Registration:** Register a new admin user.
- **Guest Registration:** Register a new guest user.
- **User Login:** Log in a user.

### **Amenities Management:**
- **Get Amenity by ID:** Retrieve a specific amenity using its unique identifier.
- **Update Amenity:** Modify an existing amenity's details.
- **Delete Amenity:** Remove an amenity from the system.
- **Create Amenity:** Add a new amenity to the system.
- **List Amenities:** Retrieves a paginated and optionally sorted list of amenities based on query parameters.
  
### Booking Management:
- **Create Booking:** Allows users to create a new booking, and receive the invoice by email.
- **Get Booking:** Retrieve details of a specific booking by its ID.
- **Delete Booking:** delete an existing booking.
- **Get Invoice:** Retrieve a booking invoice.
- **Download Invoice as PDF:** Download the invoice of a booking in PDF format.
  
### Guest Management:
- **Get Guest:** Retrieves details of a guest by their ID.
- **Get Guest's Bookings:** Retrieves all bookings for a specific guest.
- **Get Recently Visited Hotels:** Retrieve a list of recently visited hotels for a guest.
  
### Discount Management:
- **Create Room Discount:** create a new discount for a room.
- **Get Room Discounts:** Retrieve details of a room's discounts by its ID.
- **Get Room Discount:** Retrieve details of a room's discount by room ID, and discount ID.
- **Delete Room Discount:** Remove a discount from a room.
- **List Featured Deals:** Retrieves a collection of featured deals based on the specified count.

### City Management:
- **Create City:** Add a new city to the system by admins only.
- **Get City:** Retrieve details of a city by its ID.
- **Update City:** Modify details of an existing city.
- **Delete City:** Remove a city from the system.
- **List Cities:** Retrieve a list of cities, with optional sorting and searching, with pagination.
- **Get Trending Destinations:** Retrieve a list of most visited cities.
- **Upload City Images:** Add images to a city.

### Hotel Management:
- **Create Hotel:** Add a new hotel to the system.
- **Get Hotel:** Retrieve details of a hotel by its ID.
- **Update Hotel:** Modify details of an existing hotel.
- **Delete Hotel:** Remove a hotel from the system.
- **List Hotels:** Retrieve a list of hotels, with optional sorting and searching, with pagination.
- **Upload Hotel Images:** Add images to a hotel's profile.
- **Search-Filter Hotels:** Searches and filters hotels based on the specified criteria.
- **Add Amenity to a hotel:** Add an amenity to a specific hotel.
- **List Hotel Amenities:** List amenities of a hotel.

### Room Management:
- **Create Room:** Add a new room to a hotel.
- **Get Room:** Retrieve details of a room by its ID.
- **Get Rooms:** Retrieves a paginated list of rooms based on query parameters.
- **Update Room:** Modify details of an existing room.
- **Delete Room:** Remove a room from a hotel.
- **Upload Room Images:** Add images to a room's profile.
- **Add Amenity to a room:** Add an amenity to a specific room.
- **List Room Amenities:** List amenities of a room.
  
### Review Management:
- **Add Review:** Adds a review for a specific hotel.
- **Get Review:** Retrieve a specific review by its ID.
- **Get Hotel Reviews:** Retrieves the reviews for a specific hotel based on the specified query parameters.
- **Update Review:** Modify an existing review.
- **Delete Review:** Remove a review.
- **Get Average Hotel Rating:** Calculate the average rating of a hotel.

---

## Endpoints

### Identity(Authentication) Endpoints

| HTTP Method | Endpoint              | Description           |
|-------------|-----------------------|-----------------------|
| POST        | /api/register       | Register a guest      |
| POST        | /api/register-admin | Register an admin     |
| POST        | /api/login          | Login a user          |

### **Amenities Endpoints**  

| HTTP Method | Endpoint                | Description                                              |
|-------------|-------------------------|----------------------------------------------------------|
| GET         | `/api/Amenities/{id}`    | Get an amenity by its ID                                 |
| PUT         | `/api/Amenities/{id}`    | Update an amenity                                       |
| DELETE      | `/api/Amenities/{id}`    | Delete an amenity                                       |
| POST        | `/api/Amenities`         | Create a new amenity                                    |
| GET         | `/api/Amenities`         | Retrieve a paginated and sorted list of amenities       |

### Cities Endpoints

| HTTP Method | Endpoint                            | Description                              |
|-------------|-------------------------------------|------------------------------------------|
| GET         | /api/Cities/{id}                  | Get a city by its ID                     |
| DELETE      | /api/Cities/{id}                  | Delete a city                            |
| PUT         | /api/Cities/{id}                  | Update a city                            |
| POST        | /api/Cities                       | Create a new city                        |
| GET         | /api/Cities                       | Retrieve list of cities with parameters  |
| GET         | /api/Cities/trending-destinations | Retrieve top visited cities              |
| POST        | /api/Cities/{id}/images           | Upload an image to a city                |

### Hotels Endpoints

| HTTP Method | Endpoint                       | Description                           |
|-------------|--------------------------------|---------------------------------------|
| GET         | /api/Hotels/{id}             | Get a hotel by its ID                 |
| DELETE      | /api/Hotels/{id}             | Delete a hotel                        |
| PUT         | /api/Hotels/{id}             | Update a hotel                        |
| POST        | /api/Hotels                  | Create a new hotel                    |
| GET         | /api/Hotels                  | Retrieve list of hotels with params   |
| POST        | /api/Hotels/{id}/images      | Upload an image to a hotel            |
| GET         | /api/Hotels/search-filter    | Search and filter hotels              |
| POST        |/api/Hotels/{hotelId}/amenities/{amenityId}| add an amenity to a hotel|
| GET         |/api/Hotels/{hotelId}/amenities |Retrieves a list of amenities for a hotel with parameters.|

### Rooms Endpoints

| HTTP Method | Endpoint                  | Description                                  |
|-------------|---------------------------|----------------------------------------------|
| GET         | /api/Rooms/{id}         | Get a room by its ID                         |
| DELETE      | /api/Rooms/{id}         | Delete a room                                |
| PUT         | /api/Rooms/{id}         | Update a room                                |
| POST        | /api/Rooms              | Create a new room                            |
| GET         | /api/Rooms              | Retrieve a list of rooms with parameters     |
| POST        | /api/Rooms/{id}/images  | Upload an image to a room                    |
| POST        |/api/Rooms/{roomId}/amenities/{amenityId}| add an amenity to a room|
| GET         |/api/Rooms/{roomId}/amenities |Retrieves a list of amenities for a room with parameters.|

### Bookings Endpoints

| HTTP Method | Endpoint                          | Description                           |
|-------------|-----------------------------------|---------------------------------------|
| GET         | /api/Bookings/{id}              | Get a booking by its ID               |
| DELETE      | /api/Bookings/{id}              | Delete a booking                      |
| POST        | /api/Bookings                   | Create a new booking                  |
| GET         | /api/Bookings/{id}/invoice      | Retrieve an invoice for a booking     |
| GET         | /api/Bookings/{id}/pdf          | Download booking invoice as PDF       |

### Reviews Endpoints

| HTTP Method | Endpoint                                | Description                                           |
|-------------|-----------------------------------------|-------------------------------------------------------|
| POST        | /api/hotels/{hotelId}/reviews         | Adds a review for a specific hotel                    |
| GET         | /api/hotels/{hotelId}/reviews         | Retrieves reviews for a specific hotel                |
| GET         | /api/hotels/{hotelId}/reviews/{reviewId} | Get a specific review by its ID                      |
| PUT         | /api/hotels/{hotelId}/reviews/{reviewId} | Update a specific review                             |
| DELETE      | /api/hotels/{hotelId}/reviews/{reviewId} | Delete a specific review                             |
| GET         | /api/hotels/{hotelId}/reviews/average | Get the average rating of a hotel                     |

### Discounts Endpoints

| HTTP Method | Endpoint                               | Description                             |
|-------------|----------------------------------------|-----------------------------------------|
| POST        | /api/rooms/{roomId}/discounts        | Create a new discount for a room        |
|GET          |/api/rooms/{roomId}/discounts         | Get discounts for a room by its ID
| GET         | /api/rooms/{roomId}/discounts/{id}   | Get a discount by its ID                |
| DELETE      | /api/rooms/{roomId}/discounts/{id}   | Delete a discount                       |
| GET         | /api/rooms/featured-deals            | Retrieves a collection of featured deals|

### Guest Endpoints

| HTTP Method | Endpoint                                      | Description                                           |
|-------------|-----------------------------------------------|-------------------------------------------------------|
| GET         | /api/Guests/{guestId}                         | Retrieves details of a guest by their ID
| GET         | /api/Guests/{guestId}/bookings                | Retrieves all bookings for a specific guest
| GET         | /api/Guests/{guestId}/recently-visited-hotels| Retrieves recently visited hotels for a specific guest |
| GET         | /api/Guests/recently-visited-hotels         | Retrieves recently visited hotels for the current guest|

---

## Tools and Concepts  

This section provides an overview of the key tools, technologies, and concepts used in the development and operation of the Hotel Booking System API.  

### Programming Languages and Frameworks  
- **C#**: Primary programming language used.  
- **.NET 8.0**: Framework for building high-performance, cross-platform web APIs.  

### Database  
- **Entity Framework Core**: Object-relational mapping (ORM) framework for .NET, used for database interactions.  
- **SQL Server**: Database management system used for storing all application data.  
- **Azure SQL Database**: Cloud-based SQL Server instance used for deployment.  

### Deployment  
- **Azure App Service**: Hosting platform used for deploying and managing the API.  

The code-first approach enhances the project's flexibility, making it easier to evolve the database schema alongside application development and maintain version control over database changes. The system is designed for scalability and reliability by leveraging Azure services.  

### API Documentation and Design  
- **Swagger/OpenAPI**: Used for API specification and documentation.  
- **Swagger UI**: Provides a web-based UI for interacting with the API's endpoints.  
- **Postman Documentation**: A detailed API reference is available on Postman for testing and exploring API functionalities: [Postman API Docs](https://documenter.getpostman.com/view/27912762/2sAYX3q3Kx).  
- **SwaggerHub Documentation**: The API specification is also hosted on SwaggerHub for structured and standardized documentation: [SwaggerHub API Docs](https://app.swaggerhub.com/apis-docs/raghadhanon-609/hotel-booking_blatform_api/v1).  

This ensures comprehensive API documentation, making it easy for developers to test, understand, and integrate with the system.  

### Authentication and Authorization
- **JWT (JSON Web Tokens)**: Method for securely transmitting information between parties as a JSON object.
- **Microsoft.Identity**: For identity management, integrate Microsoft.Identity, a part of the Microsoft identity platform. This framework provides a comprehensive set of functionalities for handling user authentication and authorization in .NET applications. It simplifies managing user identities, securing passwords, and implementing token-based authentication.

### Testing
- **xUnit**: Unit testing tool for the C# programming language.
- **Postman**: Tool for API testing and exploring. Here is a set of API tests [Postman Collection](https://documenter.getpostman.com/view/27912762/2sAYX3q3Kx)

### Monitoring and Logging
- **Serilog**: Logging library for .NET applications.
- **Application Insights**: Extensive logging, monitoring, and alerting system.

### Security
- **HTTPS**: Ensuring secure communication over the network.
- **Data Encryption**: Encrypting sensitive data in the database.

---

## Project Architecture  

The **Hotel Booking Platform** follows a clean and modular architecture, ensuring scalability, maintainability, and separation of concerns. The solution consists of multiple projects, each responsible for a specific layer of the application.  

### **Project Structure**  

1. **HotelBookingPlatform.API** (Presentation Layer)  
   - This is the entry point of the application, exposing RESTful APIs for clients.  
   - It handles HTTP requests, routing, and API documentation using Swagger/OpenAPI.  
   - Uses controllers to interact with the application layer.  

2. **HotelBookingPlatform.Application** (Application Layer)  
   - Contains business logic and service implementations.  
   - Defines use cases and application services.  
   - Acts as an intermediary between the API and Domain layer.  

3. **HotelBookingPlatform.Domain** (Domain Layer)  
   - Contains core business entities, domain models, and interfaces.  
   - Implements domain-driven design (DDD) principles.  
   - Ensures that business rules are independent of infrastructure concerns.  

4. **HotelBookingPlatform.Infrastructure** (Infrastructure Layer)  
   - Handles data persistence using **Entity Framework Core** with **SQL Server**.  
   - Manages external dependencies such as database connections, logging, and third-party integrations.  
   - Implements repository patterns for data access.  

5. **Test** (Testing Layer)  
   - Includes unit and integration tests for different layers of the application.  
   - Uses **xUnit** framework for testing.  

This structured architecture follows best practices like **separation of concerns, dependency inversion, and clean architecture**, ensuring a well-organized and maintainable system.  

---
    
## Setup Guide

### Manual Setup
1. **Clone the Repository**: Clone the GitHub repository to your local machine.
   
   ```bash
   git clone <repository-url>
   ```
   
   Replace `<repository-url>` with your GitHub repository URL.

2. **Run the Application**: Navigate to the project directory and run the following command:
   
   ```bash
   dotnet run
   ```
   
   This will start the application locally.


### Continuous Integration, Delivery, and Deployment

The Hotel Booking Platform API uses a streamlined CI/CD pipeline, leveraging GitHub Actions and Azure Web App for efficient and reliable software delivery.

#### CI/CD Pipeline Overview

- **GitHub Actions**: Automatically manages the build and test processes. The pipeline is triggered on every push to the main branch and via manual workflow dispatch.
- **Build Process**: The application is compiled using .NET 8, ensuring that the code integrates correctly across different platforms (Ubuntu, Windows, macOS).
- **Automated Testing**: After building, the pipeline runs automated tests to verify the application's functionality and maintain code quality.
- **Artifact Packaging**: Successfully built and tested code is packaged into an artifact, ready for deployment.

#### Deployment Process

- **Azure Web App Deployment**: The application is deployed to **Azure Web App**, a platform for hosting web applications. The live version of the API is automatically updated and can be accessed at [https://hotelbookingblatform-ajewdsbpbwetccg5.canadacentral-01.azurewebsites.net/](https://hotelbookingblatform-ajewdsbpbwetccg5.canadacentral-01.azurewebsites.net/).
- **Publish Profile**: Azure Publish Profile is used for secure deployments.
  
This CI/CD approach simplifies the development process, automates testing and deployment, and ensures that new changes are efficiently and reliably delivered.

---

### Contact and Support:
For any questions, suggestions, or discussions, please feel free to reach out at [raghadhanoon2015@gmail.com](raghadhanoon2015@gmail.com)