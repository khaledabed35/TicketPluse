# TicketPluse 🎟️✨

A robust ticketing and seating management system built with **.NET Core Web API**, following best practices and clean software architecture.

## 🛠️ Key Features & Tech Stack
* **Architecture:** Structured using Layered Architecture (**BLL & DAL**) to ensure separation of concerns and maintainability.
* **Performance Optimization:** Integrated **Redis Cache** to cache frequently accessed data (like event details and seat availability), significantly reducing database load and optimizing API response times.
* **Authentication & Authorization:** Secure User Management and Auth system featuring JWT, Role-based access control (Admin/User), Email Confirmation, and Password Reset workflows.
* **Core Logic:** Comprehensive endpoints for managing **Events, Seats, and Bookings**.
* **Payment Integration:** Secure checkout flow and real-time payment processing using **Stripe** integration, handling asynchronous events securely via **Webhooks**.
* **API Testing:** Fully documented with Swagger and thoroughly tested using Postman.

## 📊 Database Design
Here is the Entity-Relationship Diagram (ERD) representing the database architecture:

![Database Diagram](./Untitled%20Diagram.drawio.png)
