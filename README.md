# LudusLogicae

**LudusLogicae** is an AI debate platform where multiple LLM-powered members engage in structured debates with transparent scoring and performance tracking.

## Core Principles
- Enable multi-agent debate sessions with configurable members
- Track member performance across debates (wins, points, participation)
- Provide transparent voting and scoring mechanisms
- Support debate orchestration with multiple rounds and phases
- Feature-based architecture for maintainability
- Clean separation of concerns (validation, orchestration, processing)
- Explicit state management with full debate state persistence
- Member-centric design with stats tracking and caching
- Provider-agnostic LLM integration with fallback support

## Debate Flow

The debate system implements a 9-phase structured flow (with automatic transitions):

### 1. Created
- Debate initialized with question, context, number of members and rounds
- Configuration stored, ready for execution

### 2. InitialAnswer
- Each member provides their initial position on the question
- Responses are independently generated based on member configuration

### 3. CrossQuestion
- Members ask each other questions
- Phase repeats for N rounds (configurable)
- Targeted questioning to explore positions and assumptions

### 4. Response
- Members respond to questions directed at them
- Full debate context is available for informed responses

### 5. Amendment
- Members can amend their initial positions based on discussion
- Opportunity to refine arguments based on exchanges

### 6. FinalAnswer
- Each member presents their final position
- Includes refined arguments and conclusions

### 7. Ballot
- Members vote using ranked ballots
- Democratic evaluation of arguments using Borda count

### 8. Completed
- Winner determined via Borda count scoring
- Results and full state persisted to database

### 9. Failed
- Debate execution encountered an error and stopped
- Error details logged for debugging

## Stack
- **Backend**: .NET 10 Web API with EF Core
- **Frontend**: React 19 + TypeScript + Vite
- **Data**: PostgreSQL with JSONB for state/results storage
- **LLM Integration**: Provider-agnostic with fallback support (OpenAI, etc.)
- **Deployment**: Docker + docker-compose (API + PostgreSQL + Frontend)

## Frontend

### Stack
- **Framework**: React 19 + TypeScript
- **Build Tool**: Vite 7
- **Styling**: CSS Modules + CSS Custom Properties
- **Animations**: anime.js
- **Routing**: React Router v6
- **Icons**: Lucide React

### Architecture
```
frontend/
├── public/
│   └── favicon.svg          # LL logo favicon
├── src/
│   ├── api/                  # API client
│   ├── components/
│   │   ├── layout/           # Layout, NavButton, PageHeader
│   │   └── ui/               # Reusable UI components
│   ├── hooks/                # Custom React hooks
│   ├── pages/
│   │   ├── debates/          # Debate pages
│   │   ├── members/          # Member pages
│   │   ├── Dashboard.tsx     # Main dashboard
│   │   └── LandingPage.tsx   # Entry gate animation
│   ├── types/                # TypeScript types
│   ├── App.tsx               # Root component & routing
│   └── index.css             # Global styles & variables
├── Dockerfile                # Production container
├── nginx.conf                # Nginx configuration
└── package.json
```

### Running Frontend

#### Development
```bash
cd frontend
npm install
npm run dev
```
Access: `http://localhost:3000`

#### Production (Docker)
```bash
docker compose up frontend
```
Access: `http://localhost:3000`

### Features
- **Landing Page**: Animated wrought iron gate entrance
- **Dashboard**: Stats overview, top performers, recent debates
- **Members**: CRUD with search, filter, sort, introduction generation
- **Debates**: Create wizard, detail view, animated replay mode
- **Navigation**: Collapsible nav button with breadcrumbs
- **Animations**: Scene-based loading overlays, page transitions

## Architecture

### Feature-Based Project Structure
```
src/LudusLogicae.Api/
├── Core/
│   ├── Common/
│   │   └── Constants/           # DebateConstants, MemberConstants, PaginationConstants
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
├── Features/
│   ├── Dashboard/
│   │   ├── Controllers/         # DashboardController
│   │   └── Contracts/           # DashboardStatsResponse
│   ├── Debates/
│   │   ├── Controllers/         # DebatesController
│   │   ├── Services/
│   │   │   ├── IDebateService.cs
│   │   │   ├── DebateService.cs
│   │   │   ├── Data/            # DebateRepository (DB queries & state persistence)
│   │   │   ├── Orchestration/   # DebateOrchestrator, DebateStepExecutor (phase execution)
│   │   │   └── Results/         # DebateResultCalculator, DebateWinnerBuilder
│   │   ├── Entities/            # Debate, DebateParticipant, DebatePhase
│   │   ├── Contracts/           # CreateDebateRequest, UpdateDebateRequest, DebateResponse, etc.
│   │   ├── Validation/          # DebateValidator, DebateValidationHelper
│   │   ├── Factories/           # DebateFactory
│   │   └── Models/              # DebateState, DebatePhaseModels, RoundModels (internal state)
│   ├── Members/
│   │   ├── Controllers/         # MembersController
│   │   ├── Services/
│   │   │   ├── IMembersService.cs
│   │   │   ├── MembersService.cs
│   │   │   ├── Data/            # MemberRepository, MemberIntroductionCacheRepository
│   │   │   ├── MemberSelectionService.cs
│   │   │   ├── MemberIntroductionFetcher.cs
│   │   │   ├── MemberStatsUpdater.cs
│   │   │   └── MemberSortHelper.cs
│   │   ├── Entities/            # Member, MemberIntroductionCache, MemberSortOption, MemberStatus
│   │   ├── Contracts/           # MemberDto, CreateMemberRequest, UpdateMemberRequest, etc.
│   │   └── Validation/          # MemberValidator
│   ├── Metadata/
│   │   ├── Controllers/         # MetadataController
│   │   ├── Services/
│   │   │   ├── IMetadataService.cs
│   │   │   └── MetadataService.cs
│   │   └── Contracts/           # MetadataInfoResponse, StackInfo
├── Infrastructure/
│   ├── Llm/
│   │   ├── ILLMClient.cs
│   │   ├── ProviderSelectingLLMClient.cs
│   │   ├── Models/              # LLMRequest, LLMResponse, LLMMessage, etc.
│   │   ├── OpenAI/              # OpenAIResponsesClient, OpenAISettings
│   │   └── Prompts/             # DebatePromptFactory, MemberPromptFactory
│   └── Data/
│       ├── AppDbContext.cs      # EF Core DbContext
│       ├── Migrations/          # EF Core migrations
│       └── SeedData/            # Initial member seed data
└── Shared/
    ├── Contracts/               # LudusLogicaeResponse, ApiResponse, ResponseFactory
    └── Constants/               # Shared constants
```

### Code Quality Standards
- **One Class Per File**: Each class/interface in its own file for better navigation
- **Separated Interfaces**: All interfaces separated from implementations
- **No Region Tags**: Code flows naturally without artificial sections
- **No Inline Comments**: Self-documenting code with clear method names
- **XML Documentation**: Comprehensive API documentation on controllers and public APIs
- **No Void Methods**: All repository/service methods return values for better testability
- **DRY Principle**: Reusable helper methods and factories
- **Clean Formatting**: Consistent code style via `dotnet format`

### Implemented Components

#### Member Management
- **CRUD Operations**: Create, read, update, delete members
- **Member Configuration**: Name, role, style hints, behavior extras, LLM settings
- **Stats Tracking**: Points, debates count, wins count, last place count, last debate date
- **Introduction Generation**: LLM-powered member introductions with caching
- **Search & Sort**: Filtering, pagination, and multiple sort options (by name, points, wins, debates, creation date)
- **Validation**: Temperature range validation, clean separation from data annotations
- **Seeded Members**: 30+ pre-configured historical figures, philosophers, scientists, and fictional characters

#### Debate System
- **9-Phase Debate Flow**: Created → InitialAnswer → CrossQuestion → Response → Amendment → FinalAnswer → Ballot → Completed/Failed
- **DebateService**: High-level CRUD and orchestration
- **DebateRepository**: Centralized database queries and state persistence
- **DebateOrchestrator**: Phase execution logic and LLM orchestration
- **DebateStepExecutor**: Wraps orchestrator calls, returns phase data
- **Result Calculation**: DebateResultCalculator for Borda count scoring
- **Winner Building**: DebateWinnerBuilder for winner determination
- **State Persistence**: Full debate state stored as JSONB in PostgreSQL
- **Participant Management**: Track member participation with stats updates

#### LLM Integration
- **Provider-Agnostic Design**: Support for multiple LLM providers
- **Fallback Support**: Automatic fallback when provider/model unavailable
- **OpenAI Implementation**: OpenAIResponsesClient with configurable models
- **Prompt Factories**: DebatePromptFactory and MemberPromptFactory for structured prompts
- **Response Parsing**: Token tracking and usage monitoring

#### Dashboard
- **Platform Statistics**: Aggregated counts for members and debates
- **Member Stats**: Total, active, benched, retired counts
- **Debate Stats**: Total, ready, completed, failed counts

#### Infrastructure
- **Feature-Based Architecture**: Code organized by business feature (Dashboard, Debates, Members, Metadata)
- **Response Factory**: Centralized error handling and consistent API responses
- **EF Core with PostgreSQL**: Full migration support with JSONB for complex state
- **Introduction Caching**: MemberIntroductionCache for LLM response caching
- **Health Checks**: Liveness probes and system metadata endpoints
- **Docker Support**: docker-compose with API and PostgreSQL services



## API Endpoints

### Members
- `GET /api/members` — List members with filtering, pagination, and sorting
  - Query params: `search`, `status`, `sortBy`, `page`, `pageSize`
  - Sort options: CreatedDesc/Asc, PointsDesc/Asc, WinsDesc/Asc, DebatesDesc/Asc, NameAsc/Desc
- `GET /api/members/{id}` — Get member by ID
- `POST /api/members` — Create a new member
- `PUT /api/members/{id}` — Update a member
- `DELETE /api/members/{id}` — Delete a member
- `POST /api/members/{id}/introductions` — Generate member introduction via LLM
  - Returns cached result if available

### Debates
- `GET /api/debates` — List debates with filtering and pagination
- `GET /api/debates/{id}` — Get full debate details including state
  - Returns complete debate state for completed debates (all rounds, votes, winner)
- `POST /api/debates` — Create a new debate
- `PUT /api/debates/{id}` — Update debate (only if in Created phase)
- `DELETE /api/debates/{id}` — Delete debate (only if in Created phase)
- `POST /api/debates/{id}/fully-run` — Execute complete debate flow

### Dashboard
- `GET /api/dashboard/stats` — Get platform statistics
  - Returns: member counts (total, active, benched, retired) and debate counts (total, ready, completed, failed)

### Metadata
- `GET /api/metadata/ping` — Liveness probe
- `GET /api/metadata/info` — System information and architecture details
- `GET /api/metadata/dictionary/provider` — List configured LLM providers
- `GET /api/metadata/dictionary/provider/{id}/models` — List models for provider
- `GET /api/metadata/dictionary/debate-phase` — List debate phases
- `GET /api/metadata/dictionary/member-sort-options` — List member sort options with descriptions

## Database Schema

### Core Entities
- **Member**: Id, Name, Role, Provider, Model, Temperature, StyleHint, BehaviorExtras, Status, Points, DebatesCount, WinsCount, LastPlaceCount, LastDebateDate, CreatedAt
  - Indexed on: Name, Status, Points, WinsCount, DebatesCount, CreatedAt
- **MemberIntroductionCache**: Id, MemberId, Provider, Model, Temperature, Introduction, CreatedAt
  - Unique index on: (MemberId, Provider, Model, Temperature)
- **Debate**: Id, Question, Context, NMembers, NRounds, Phase, State (JSONB), CreatedAt, CompletedAt
  - Indexed on: Phase, CreatedAt, CompletedAt
- **DebateParticipant**: Id, DebateId, MemberId, SlotIndex, Name, Role, Model, Temperature
  - Unique index on: (DebateId, SlotIndex)
  - Foreign keys: DebateId → Debates (cascade delete), MemberId → Members (restrict)
- **GeneralPrompt**: Id, PromptName, Content

### State Storage
- **Debate.State**: Full debate state stored as JSONB
  - All member initial answers, questions, responses, amendments, final statements
  - All ballots and voting data
  - Calculated results, scores, and rankings
  - Winner information (if determined)
  - Tie-break votes (if applicable)
  - System prompts used for each phase

## Running Locally

### Using Docker Compose
```powershell
# From repo root
# 1. Copy .env.example to .env and configure your OpenAI API key
cp .env.example .env

# 2. Edit .env and set your OPENAI_API_KEY

# 3. Start the services
docker compose up --build
```

Access:
- Frontend: `http://localhost:3000`
- Swagger: `https://localhost:8443/swagger`
- HTTP: `http://localhost:8080/swagger`
- Health: `https://localhost:8443/health-check`

### Using Rider
1. Run → Edit Configurations → "+" → Docker Compose
2. Add `docker-compose.yml`
3. Select services: `api`, `db`
4. Check "Build images"
5. Configure environment variables via `.env` file
6. Apply → Run

## Roadmap
- **v0**: Scaffold, API, Dockerfile, PostgreSQL
- **v1**: EF Core, debate engine, member management, LLM integration
- **v2**: Feature-based architecture, debate flow with 9 phases, Borda voting, member stats tracking
- **v2.1**: Code cleanup, refactoring, and quality improvements

## Recent Major Updates (v2.1)

### Code Quality & Organization
- **Separated Files**: Each class/interface in its own file
  - Split multi-class contract files (DebateContracts, ApiResponse)
  - Separated all interfaces from implementations
- **Removed Clutter**: Eliminated all region tags and inline comments
- **Clean Code**: Self-documenting code with comprehensive XML documentation
- **Consistent Formatting**: Applied `dotnet format` across entire codebase

### Architecture Improvements
- **Repository Pattern**: Fully implemented for both Debates and Members
  - `DebateRepository`: Centralized database operations for debates
  - `MemberRepository`: Consistent data access pattern for members
  - `MemberIntroductionCacheRepository`: Dedicated cache operations
- **Validation Consolidation**: Reduced 3 duplicate validation methods to 1 generic method
  - Created `DebateOperationType` enum for type-safe validation
  - Single `ValidateEditability<T>` method handles all debate operations
- **Named Record Types**: Replaced anonymous objects with 7 internal record types in `DebateOrchestrator`
  - `InitialAnswerPhaseInput`, `RoundQuestionPhaseInput`, `RoundAnswerPhaseInput`, etc.
  - Improved type safety and code readability
- **No Void Methods**: All repository methods return values for better testability
- **DRY Improvements**: Reduced code duplication through helper methods and consolidation

### Performance & Reliability
- **Thread-Safe Concurrency**: Replaced lock-based synchronization with `ConcurrentBag<T>`
  - Eliminated lock contention in parallel LLM response generation
  - Better performance for concurrent debate phase execution
- **Exponential Backoff**: Improved migration retry logic from linear to exponential backoff
  - Faster recovery on early attempts (500ms → 1s → 2s → 4s → 5s)
  - Reduced database load during retries
- **Ballot Validation**: Added validation to prevent invalid ballot data from corrupting scores
  - Validates slot ranges, detects duplicates, ensures data integrity
- **Diff-Based Updates**: Optimized participant replacement strategy
  - Calculates minimal changes (add/remove/update) instead of delete-all-insert-all
  - Reduced database operations and improved performance
- **DI Lifetime Optimization**: Corrected service lifetimes (Singleton for stateless strategies)

### Bug Fixes
- **Round Pairing Algorithm**: Fixed critical bug causing pairing repetition after N-1 rounds
  - Changed from `offset == 0 ? 1 : offset` to proper modulo calculation
  - Ensures unique pairings across all debate rounds

### Code Simplification
- **Introduction Cache Logic**: Simplified complex change tracker manipulation
  - Replaced with clean early-return pattern
  - Removed manual entity state management
- **Query Composition**: Verified EF Core query composition follows best practices
  - Multiple `Where` clauses properly compose into single SQL query

### API Enhancements
- **Enhanced GetById**: Returns full debate state including all rounds, votes, and winner
- **Removed GetMembers**: Simplified API surface (state included in GetById)
- **Explicit Participant Deletion**: Debates properly clean up all related data

### Database & State Management
- **Centralized State Persistence**: All state operations through repository pattern
- **Cascade Delete**: Proper cleanup of debate participants
- **Return Values**: Repository operations return updated entities

### Code Metrics
- **Lines Reduced**: ~200+ lines eliminated through DRY improvements and consolidation
- **Files Organized**: 2 new repository classes, improved separation of concerns
- **Build Status**: Zero errors, zero warnings after all refactoring

## Contributing
Issues and PRs welcome. Please keep changes small and focused.

## License
This project is licensed under the [MIT License](LICENSE).
