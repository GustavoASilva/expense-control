# Expense Control - React Frontend

A modern React frontend for the Expense Control application, built with:

- **React 19** with TypeScript
- **Vite** for fast development and building
- **React Router** for navigation
- **Axios** for API communication
- **Bootstrap 5** for styling
- **Chart.js** with react-chartjs-2 for data visualization

## Features

- ✅ Dashboard with real-time statistics
- ✅ Income/Expense tracking with visual charts
- ✅ Transaction management (Create, Read, Update, Delete)
- ✅ Category-based organization
- ✅ Date range filtering
- ✅ Responsive design with Bootstrap
- ✅ TypeScript for type safety

## Prerequisites

- Node.js 20.x or later
- npm 10.x or later
- Backend API running on http://localhost:5293

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure Environment

Create a `.env` file in the root directory:

```env
VITE_API_URL=http://localhost:5293/api
```

### 3. Start Development Server

```bash
npm run dev
```

The application will be available at http://localhost:5173

### 4. Build for Production

```bash
npm run build
```

The production-ready files will be in the `dist` directory.

## Project Structure

```
expense-control-react/
├── src/
│   ├── components/          # Reusable React components
│   │   ├── DateRangePicker.tsx
│   │   ├── StatCard.tsx
│   │   ├── NavMenu.tsx
│   │   └── Layout.tsx
│   ├── pages/              # Page components
│   │   ├── Dashboard.tsx
│   │   ├── Transactions.tsx
│   │   └── TransactionForm.tsx
│   ├── services/           # API service layer
│   │   └── api.ts
│   ├── types/              # TypeScript type definitions
│   │   ├── index.ts
│   │   └── models.ts
│   ├── App.tsx             # Main app component with routing
│   ├── main.tsx            # Application entry point
│   └── index.css           # Global styles
├── public/                 # Static assets
├── .env                    # Environment configuration
├── vite.config.ts          # Vite configuration
├── tsconfig.json           # TypeScript configuration
└── package.json            # Dependencies and scripts
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## API Integration

The application communicates with the backend API through Axios. All API calls are centralized in `src/services/api.ts`.

### Endpoints Used

- `GET /api/transactions` - List transactions
- `GET /api/transactions/{id}` - Get transaction details
- `POST /api/transactions` - Create transaction
- `PATCH /api/transactions/{id}` - Update transaction
- `DELETE /api/transactions/{id}` - Delete transaction
- `GET /api/categories` - List categories
- `GET /api/balance` - Get balance summary
- `GET /api/balance/by-category` - Get balance by category
- `GET /api/balance/monthly` - Get monthly balance data

## Development Notes

### TypeScript Configuration

The project uses strict TypeScript settings. If you encounter issues with type imports, ensure your `tsconfig.app.json` does not have `verbatimModuleSyntax` or `erasableSyntaxOnly` enabled, as these can cause issues with enum exports.

### Styling

The application uses Bootstrap 5 for styling with custom CSS overrides in `src/index.css` to match the original Blazor design, including:
- Purple/blue gradient sidebar
- Consistent color scheme
- Responsive layout

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## License

This project is for educational and development purposes.
