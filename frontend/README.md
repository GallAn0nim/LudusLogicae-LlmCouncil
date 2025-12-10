# LudusLogicae Frontend

React-based frontend for the LudusLogicae AI Debate Platform.

## Quick Start

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| React | 19.2 | UI Framework |
| TypeScript | 5.9 | Type Safety |
| Vite | 7.2 | Build Tool |
| React Router | 6.30 | Routing |
| anime.js | 3.2 | Animations |
| Lucide React | 0.556 | Icons |

## Project Structure

```
src/
├── api/
│   └── client.ts           # API client with typed endpoints
├── components/
│   ├── layout/
│   │   ├── Layout.tsx      # Main layout wrapper
│   │   ├── NavButton.tsx   # Navigation button with dropdown
│   │   └── PageHeader.tsx  # Page title component
│   └── ui/
│       ├── Badge.tsx       # Status badges
│       ├── Button.tsx      # Button variants
│       ├── Card.tsx        # Card container
│       ├── DebateLoadingOverlay.tsx  # Animated loading scenes
│       ├── EmptyState.tsx  # Empty content placeholder
│       ├── Input.tsx       # Form input
│       ├── Loading.tsx     # Loading spinner
│       ├── Modal.tsx       # Modal dialog
│       ├── Select.tsx      # Dropdown select
│       └── Slider.tsx      # Range slider
├── hooks/
│   └── useApi.ts           # API fetch hooks
├── pages/
│   ├── debates/
│   │   ├── CreateDebateWizard.tsx
│   │   ├── DebateCard.tsx
│   │   ├── DebateDetailPage.tsx
│   │   ├── DebateReplayPage.tsx
│   │   └── DebatesPage.tsx
│   ├── members/
│   │   ├── MemberCard.tsx
│   │   ├── MemberDetailPage.tsx
│   │   ├── MemberForm.tsx
│   │   └── MembersPage.tsx
│   ├── Dashboard.tsx
│   ├── LandingPage.tsx
│   └── index.ts
├── types/
│   └── index.ts            # Shared TypeScript types
├── App.tsx                 # Root component & routes
├── index.css               # Global styles & CSS variables
└── main.tsx                # Entry point
```

## Routes

| Path | Component | Description |
|------|-----------|-------------|
| `/` | LandingPage | Animated gate entrance |
| `/dashboard` | Dashboard | Stats overview |
| `/members` | MembersPage | Member list |
| `/members/:id` | MemberDetailPage | Member details |
| `/debates` | DebatesPage | Debate list |
| `/debates/:id` | DebateDetailPage | Debate details |
| `/debates/:id/replay` | DebateReplayPage | Animated replay |

## Styling

### CSS Variables

All colors, spacing, and effects use CSS custom properties defined in `index.css`:

```css
/* Colors */
--bg-primary, --bg-secondary, --bg-tertiary
--text-primary, --text-secondary, --text-tertiary
--accent-primary, --accent-secondary
--success, --warning, --error, --info

/* Spacing via CSS Modules */
/* Animations via anime.js */
```

### CSS Modules

Each component has a co-located `.module.css` file for scoped styling.

## API Integration

The API client (`src/api/client.ts`) provides typed methods for all endpoints:

```typescript
// Members
membersApi.list(query)
membersApi.get(id)
membersApi.create(data)
membersApi.update(id, data)
membersApi.delete(id)
membersApi.getIntroduction(id)

// Debates
debatesApi.list(query)
debatesApi.get(id)
debatesApi.create(data)
debatesApi.update(id, data)
debatesApi.delete(id)
debatesApi.run(id)

// Dashboard
dashboardApi.getStats()
```

## Docker

### Build Image

```bash
docker build -t luduslogicae-frontend .
```

### Run Container

```bash
docker run -p 3000:80 luduslogicae-frontend
```

### With Docker Compose

```bash
# From repo root
docker compose up frontend
```

## Development

### Adding a New Page

1. Create component in `src/pages/`
2. Add route in `App.tsx`
3. Export from `src/pages/index.ts`
4. Add navigation link in `NavButton.tsx`

### Adding a New UI Component

1. Create component in `src/components/ui/`
2. Create co-located `.module.css`
3. Export from `src/components/ui/index.ts`

## Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Start dev server on port 3000 |
| `npm run build` | Build for production |
| `npm run preview` | Preview production build |
| `npm run lint` | Run ESLint |

## Environment

The API base URL is configured in `src/api/client.ts`. For local development, it defaults to `http://localhost:8080`.
