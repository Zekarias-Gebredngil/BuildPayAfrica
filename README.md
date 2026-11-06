# BuildPay Africa

A comprehensive construction procurement, project management, and payments platform designed for the African market. BuildPay Africa streamlines the entire construction lifecycle from contractor discovery to milestone-based payments with escrow protection.

## 🌟 Features

### For Customers
- **Project Management**: Create and manage construction projects with detailed specifications
- **Milestone Tracking**: Track project progress with visual milestone indicators
- **Escrow Payments**: Secure milestone-based payments with release approval
- **Contractor Search**: Find and hire verified contractors with ratings and reviews
- **Real-time Updates**: Monitor project progress and delivery status

### For Contractors
- **Profile Management**: Showcase skills, certifications, and portfolio
- **Project Discovery**: Browse and bid on available projects
- **Payment Tracking**: Monitor escrow payments and milestone releases
- **Verification System**: Get verified to build trust with customers
- **Communication**: Direct messaging with customers

### For Suppliers
- **Inventory Management**: Manage product stock with low-stock alerts
- **Order Fulfillment**: Process and track customer orders
- **Analytics Dashboard**: View sales reports and top-selling products
- **Delivery Configuration**: Set up delivery zones and pricing

### For Drivers
- **Delivery Tracking**: Real-time delivery status updates
- **Route Optimization**: Efficient delivery route planning
- **Earnings Tracking**: Monitor delivery earnings and history

### For Administrators
- **User Management**: Manage users with role-based access control
- **Verification Approvals**: Review and approve contractor/supplier verifications
- **Transaction Monitoring**: Monitor all platform transactions
- **Analytics Dashboard**: View platform metrics and user growth
- **System Health**: Monitor system performance and alerts

## 🏗️ Architecture

BuildPay Africa uses a **microservices architecture** with the following components:

### Backend Services
- **API Gateway** (YARP): Routes requests to appropriate microservices
- **Identity Service**: User authentication and authorization (Keycloak integration)
- **Project Service**: Project management and milestone tracking
- **Payment Service**: Escrow wallet and payment processing
- **Marketplace Service**: Product catalog and order management
- **Delivery Service**: Delivery tracking and driver management
- **Notification Service**: Email, SMS, and push notifications
- **User Profile Service**: User profile and verification management
- **Monolith API**: Legacy API (being migrated to microservices)

### Frontend
- **Angular 19**: Modern SPA with TypeScript
- **Angular Material**: UI component library
- **TailwindCSS**: Utility-first CSS framework
- **Responsive Design**: Mobile-first approach

### Infrastructure
- **PostgreSQL 16**: Primary database
- **Redis 7**: Caching and session management
- **RabbitMQ**: Message broker for async communication
- **Keycloak**: Identity and access management
- **Jaeger**: Distributed tracing
- **Prometheus**: Metrics collection
- **Grafana**: Metrics visualization

## 🛠️ Tech Stack

### Backend
- **.NET 8**: C# with ASP.NET Core
- **Entity Framework Core**: ORM for database operations
- **MediatR**: CQRS pattern implementation
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **OpenTelemetry**: Distributed tracing

### Frontend
- **Angular 19**: TypeScript framework
- **Angular Material 19**: UI components
- **TailwindCSS 4**: Styling
- **RxJS**: Reactive programming
- **Angular Router**: Client-side routing

### DevOps
- **Docker**: Containerization
- **Docker Compose**: Local development orchestration
- **GitHub Actions**: CI/CD pipeline

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

- **Docker** & **Docker Compose** (for containerized setup)
- **.NET 8 SDK** (for local development without Docker)
- **Node.js 18+** & **npm** (for frontend development)
- **Git** (for version control)

## 🚀 Quick Start

### Using Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/Zekarias-Gebredngil/BuildPayAfrica.git
   cd BuildPayAfrica
   ```

2. **Configure environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

3. **Start all services**
   ```bash
   docker-compose up -d
   ```

4. **Access the application**
   - Frontend: http://localhost
   - API Gateway: http://localhost:8082
   - Keycloak Admin: http://localhost:8080/admin
   - Grafana: http://localhost:3000
   - Jaeger: http://localhost:16686

### Local Development

#### Backend Setup

1. **Navigate to the solution directory**
   ```bash
   cd BuildPayAfrica
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure connection strings**
   - Update `appsettings.json` in each project with your database credentials

4. **Run database migrations**
   ```bash
   dotnet ef database update --project BuildPay.Persistence
   ```

5. **Run the API**
   ```bash
   dotnet run --project BuildPay.API
   ```

#### Frontend Setup

1. **Navigate to the frontend directory**
   ```bash
   cd frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start the development server**
   ```bash
   npm start
   ```

4. **Access the application**
   - Open http://localhost:4200 in your browser

## 📁 Project Structure

```
BuildPayAfrica/
├── BuildPay.API/              # Monolith API (legacy)
├── BuildPay.Application/      # Application logic (CQRS)
├── BuildPay.Domain/           # Domain entities and interfaces
├── BuildPay.Infrastructure/   # External services integration
├── BuildPay.Persistence/      # Database context and migrations
├── BuildPay.Gateway/         # API Gateway (YARP)
├── services/                  # Microservices
│   ├── BuildPay.IdentityService/
│   ├── BuildPay.ProjectService/
│   ├── BuildPay.PaymentService/
│   ├── BuildPay.MarketplaceService/
│   ├── BuildPay.DeliveryService/
│   ├── BuildPay.NotificationService/
│   └── BuildPay.UserProfileService/
├── shared/                    # Shared libraries
│   ├── BuildPay.Contracts/
│   └── BuildPay.ServiceDefaults/
├── frontend/                 # Angular frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── features/      # Feature modules
│   │   │   ├── layout/       # Layout components
│   │   │   └── core/         # Core services
│   │   └── ...
│   └── ...
├── docker/                    # Docker configurations
├── infra/                     # Infrastructure configs
│   ├── prometheus/
│   └── grafana/
├── keycloak/                 # Keycloak realm configuration
└── docker-compose.yml        # Docker orchestration
```

## 🔧 Configuration

### Environment Variables

Key environment variables (see `.env.example`):

```env
# Database
POSTGRES_USER=buildpay
POSTGRES_PASSWORD=your_password
POSTGRES_DB=buildpay

# RabbitMQ
RABBITMQ_DEFAULT_USER=buildpay
RABBITMQ_DEFAULT_PASS=your_password

# Keycloak
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=your_password
KEYCLOAK_DB_USERNAME=keycloak
KEYCLOAK_DB_PASSWORD=your_password

# AWS (for file storage)
AWS_ACCESS_KEY_ID=your_access_key
AWS_SECRET_ACCESS_KEY=your_secret_key
AWS_REGION=us-east-1
S3_BUCKET=buildpay-uploads

# Grafana
GRAFANA_ADMIN_USER=admin
GRAFANA_ADMIN_PASSWORD=your_password
```

## 📊 Monitoring & Observability

### Grafana Dashboards
- Access at http://localhost:3000
- Default credentials: admin / your_password
- Pre-configured dashboards for:
  - System metrics
  - Application performance
  - Database performance
  - Message queue metrics

### Jaeger Tracing
- Access at http://localhost:16686
- Distributed tracing across all microservices
- Performance analysis and debugging

### Prometheus Metrics
- Access at http://localhost:9090
- Metrics collection and querying
- Alert rule configuration

## 🧪 Testing

### Backend Tests
```bash
dotnet test BuildPay.Application.Tests
```

### Frontend Tests
```bash
cd frontend
npm test
```

### E2E Tests
```bash
cd frontend
npm run e2e
```

## 🚢 Deployment

### Using Docker

1. **Build Docker images**
   ```bash
   docker-compose build
   ```

2. **Push to registry**
   ```bash
   docker-compose push
   ```

3. **Deploy to production**
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```

### Manual Deployment

See `deploy.bat` for automated deployment script which:
- Builds all .NET projects
- Publishes to output directory
- Deploys to specified environment

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Style
- **C#:** Follow .NET coding conventions
- **TypeScript:** Follow Angular style guide
- **HTML:** Use semantic HTML5
- **CSS:** Use TailwindCSS utility classes

### Commit Messages
Use conventional commits:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation changes
- `style:` Code style changes
- `refactor:` Code refactoring
- `test:` Test changes
- `chore:` Build process or auxiliary tool changes

## 📝 API Documentation

API documentation is available via Swagger UI(s):
- Monolith API: http://localhost:8081/swagger
- API Gateway: http://localhost:8082/swagger

## 🔐 Security

- **Authentication**: Keycloak OAuth2/OpenID Connect
- **Authorization**: Role-based access control (RBAC)
- **Data Encryption**: TLS in transit, encryption at rest
- **Input Validation**: FluentValidation on all endpoints
- **SQL Injection Prevention**: Parameterized queries via EF Core

## 🌐 Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Zekarias Gebredngil** - *Initial work* - [Zekarias-Gebredngil](https://github.com/Zekarias-Gebredngil)

## 🙏 Acknowledgments

- Angular team for the amazing framework
- .NET community for excellent libraries
- Keycloak for identity management solution
- All contributors who have helped shape this project

## 📞 Support

For support, email support@buildpay.africa or open an issue on GitHub.

## 🗺️ Roadmap

- [ ] Mobile applications (iOS & Android)
- [ ] Advanced analytics with ML
- [ ] Multi-currency support
- [ ] Integration with local payment gateways
- [ ] Offline mode for field workers
- [ ] Voice commands for hands-free operation
- [ ] AR/VR for project visualization

---

**Built with ❤️ for Africa's construction industry**
