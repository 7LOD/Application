# 🎯 MyEventsApi (.NET 8 Web API)

![Swagger UI](swagger-ui.png)

**MyEventsApi** — це REST API для керування подіями: користувачі можуть створювати, редагувати, приєднуватись або залишати події.  
Система побудована на **.NET 8**, використовує **Entity Framework Core**, **JWT авторизацію** та **PostgreSQL** у Docker-середовищі.

---


## 🚀 Швидкий запуск

```bash

docker compose up --build

🔗 Swagger UI: http://localhost:8080/swagger

---

⚙️ Основні можливості

🔐 JWT Auth — реєстрація, логін, авторизація

📅 Events CRUD — створення, редагування, видалення подій

🙋 Join / Leave — користувачі можуть приєднуватися до подій

🐳 Docker + PostgreSQL — повна ізоляція середовища

🧩 Health-check — для моніторингу стану API

---
🧪 API Quick Test

Після запуску контейнерів:

Swagger UI:
👉 http://localhost:8080/swagger

---

🔑 Авторизація

У Swagger або через curl додавай у заголовок:

-H "Authorization: Bearer <your_token>"


Enter 'Bearer' [space] and then your token.
Example: Bearer [token]

---

👥 Тестові користувачі (Seeder)
Email	Password
john@example.com
	123456
mary@example.com
	123456

---

📅 Приклади запитів

Отримати всі публічні події

curl -X GET "http://localhost:8080/events" -H "accept: */*"


Створити нову подію

curl -X POST "http://localhost:8080/events" \
-H "Authorization: Bearer <your_token>" \
-H "Content-Type: application/json" \
-d '{
  "title": "Community Meetup",
  "description": "Weekly dev chat",
  "date": "2025-11-15T18:00:00Z",
  "location": "Kyiv",
  "capacity": 20,
  "isPublic": true
}'


Отримати мої події (створені + приєднані)

curl -X GET "http://localhost:8080/users/me/events" \
-H "Authorization: Bearer <your_token>"


🔹 capacity — optional (якщо не задано → unlimited)
🔹 isPublic — true для відкритих подій
🔹 Організатор автоматично вважається власником події, але не учасником

Після цього можна продовжити з розділом:

---

🧠 Використані технології

ASP.NET Core 8

Entity Framework Core + PostgreSQL

JWT (JSON Web Tokens)

Docker & Docker Compose

Swagger (OpenAPI)

---

📁 Структура проекту
	
Controllers/   →  AuthController, EventsController

Models/        →  User, Event, Participant

Data/          →  ApplicationDbContext (EF Core)

Dtos/          →  DTOs для запитів/відповідей

Migrations/    →  EF Core міграції