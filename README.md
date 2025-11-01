🎯 MyEventsApi (.NET 8 Web API)

MyEventsApi — це REST API для керування подіями.
Користувачі можуть створювати, редагувати, переглядати, приєднуватись або залишати події.
API побудовано на .NET 8, використовує Entity Framework Core, JWT авторизацію та PostgreSQL у Docker-середовищі.

🚀 Швидкий старт
docker compose up --build


🔗 Swagger UI: http://localhost:8080/swagger

⚙️ Основні можливості

✅ JWT Auth — реєстрація, логін, авторизація
✅ Events CRUD — створення, редагування, видалення подій
✅ Join / Leave — користувачі можуть приєднуватись до подій
✅ Search / Calendar — пошук за назвою, описом або датою; перегляд у календарі
✅ Docker + PostgreSQL — ізольоване середовище
✅ Middleware Error Handling — уніфіковані відповіді помилок
✅ Swagger UI — інтерактивна документація

🧪 API Quick Test

Після запуску контейнерів відкрити:
👉 http://localhost:8080/swagger

🔑 Авторизація

У Swagger або через curl додавай у заголовок:

-H "Authorization: Bearer <your_token>"


Enter 'Bearer' [space] and then your token.
Example: Bearer eyJhbGciOi...

👥 Тестові користувачі (Seeder)
Email	Password
john@example.com
	123456
mary@example.com
	123456
📅 Приклади запитів
🔹 Отримати всі публічні події
curl -X GET "http://localhost:8080/events" -H "accept: */*"

🔹 Створити нову подію
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

🔹 Отримати мої події (створені + приєднані)
curl -X GET "http://localhost:8080/users/me/events" \
-H "Authorization: Bearer <your_token>"

🔹 Пошук подій (назва, опис, локація, дата)
curl -X GET "http://localhost:8080/events/search?query=Kyiv"
curl -X GET "http://localhost:8080/events/search?query=2025-12-01"

🔹 Події у календарному діапазоні
curl -X GET "http://localhost:8080/events/calendar?start=2025-11-01&end=2025-11-05"


capacity — optional (якщо не задано → unlimited)
isPublic — true для відкритих подій
Організатор не рахується учасником за замовчуванням

🧠 Використані технології

ASP.NET Core 8 (Web API)

Entity Framework Core + PostgreSQL

JWT (JSON Web Tokens)

Docker & Docker Compose

Swagger (OpenAPI)

FluentValidation

Middleware Error Handling

📁 Структура проекту
Controllers/     →  AuthController, EventsController, UsersController
Models/          →  User, Event, Participant
Data/            →  ApplicationDbContext (EF Core)
Dtos/            →  DTOs для запитів/відповідей
Utils/           →  DataHelper (UTC + DTO mapping)
Migrations/      →  EF Core міграції

🧭 Додатково

Utils/DataHelper.cs — конвертація часу в UTC + мапінг моделей у DTO

calendar та search — розширення Stage #1 для подальшого використання у фронтенді (Angular)

Swagger автоматично генерується з урахуванням Bearer авторизації