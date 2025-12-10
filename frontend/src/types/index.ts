export type MemberStatus = 'Active' | 'Benched' | 'Retired';

export type DebatePhase =
  | 'Created'
  | 'InitialAnswer'
  | 'CrossQuestion'
  | 'Response'
  | 'Amendment'
  | 'FinalAnswer'
  | 'Ballot'
  | 'Completed'
  | 'Failed';

export type MemberSortOption =
  | 'CreatedDesc'
  | 'CreatedAsc'
  | 'PointsDesc'
  | 'PointsAsc'
  | 'WinsDesc'
  | 'WinsAsc'
  | 'DebatesDesc'
  | 'DebatesAsc'
  | 'NameAsc'
  | 'NameDesc';

export interface Member {
  id: string;
  name: string;
  role: string;
  styleHint: string | null;
  behaviorExtras: string | null;
  provider: string;
  model: string;
  temperature: number;
  status: MemberStatus;
  points: number;
  debatesCount: number;
  winsCount: number;
  lastPlaceCount: number;
  lastDebateDate: string | null;
  createdAt: string;
}

export interface CreateMemberRequest {
  name: string;
  role: string;
  provider: string;
  model: string;
  temperature?: number;
  styleHint?: string;
  behaviorExtras?: string;
  status?: MemberStatus;
}

export interface UpdateMemberRequest extends CreateMemberRequest { }

export interface MemberListQuery {
  status?: MemberStatus;
  provider?: string;
  model?: string;
  searchTerm?: string;
  minWins?: number;
  minPoints?: number;
  sortBy?: MemberSortOption;
  page?: number;
  pageSize?: number;
}

export interface MembersListResponse {
  items: Member[];
  total: number;
  page: number;
  pageSize: number;
}

export interface MemberIntroductionResponse {
  memberId: string;
  provider: string;
  model: string;
  temperature: number;
  introduction: string;
}

export interface CrossQuestion {
  toSlot: number;
  question: string;
}

export interface QuestionResponse {
  fromSlot: number;
  answer: string;
}

export interface DebateParticipant {
  id: string;
  memberId: string;
  slotIndex: number;
  name: string;
  role: string;
  styleHint: string | null;
  behaviorExtras: string | null;
  provider: string;
  model: string;
  temperature: number;
  initialAnswer: string | null;
  crossQuestions: CrossQuestion[];
  questionResponses: QuestionResponse[];
  amendment: string | null;
  finalAnswer: string | null;
  ballotRanking: number[] | null;
  bordaScore: number;
  finalScore: number;
  isWinner: boolean;
}

export interface CouncilMember {
  slot: number;
  name: string;
  role: string;
  model: string;
  temperature: number;
  introduction: string;
}

export interface MemberAnswer {
  slot: number;
  text: string;
}

export interface RoundPairing {
  fromSlot: number;
  toSlot: number;
}

export interface RoundQuestion {
  fromSlot: number;
  toSlot: number;
  text: string;
}

export interface RoundAnswer {
  slot: number;
  text: string;
}

export interface RoundAmendment {
  slot: number;
  text: string;
}

export interface DebateRound {
  roundNumber: number;
  pairings: RoundPairing[];
  questions: RoundQuestion[];
  answers: RoundAnswer[];
  amendments: RoundAmendment[];
}

export interface RankEntry {
  slot: number;
  rank: number;
}

export interface MemberBallot {
  slot: number;
  ranks: RankEntry[];
}

export interface MemberScore {
  slot: number;
  score: number;
  rank: number;
}

export interface MemberVote {
  voterSlot: number;
  votedForSlot: number;
}

export interface DebateWinner {
  member: CouncilMember;
  totalPoints: number;
  finalStatement: string;
  initialAnswer: string;
  history: string[];
}

export interface DebateState {
  debateId: string;
  topic: string;
  context: string | null;
  memberCount: number;
  totalRounds: number;
  members: CouncilMember[];
  initialAnswers: MemberAnswer[];
  rounds: DebateRound[];
  finalStatements: MemberAnswer[];
  ballots: MemberBallot[];
  results: MemberScore[];
  tieBreakVotes: MemberVote[];
  winner: DebateWinner | null;
}

export interface Debate {
  id: string;
  status: DebatePhase;
  question: string;
  context: string | null;
  nMembers: number;
  nRounds: number;
  createdAt: string;
  completedAt: string | null;
  participants: DebateParticipant[];
  state: DebateState | null;
}

export interface CreateDebateRequest {
  question: string;
  nMembers?: number;
  nRounds?: number;
  context?: string;
  memberIds?: string[];
}

export interface UpdateDebateRequest {
  question?: string;
  nMembers?: number;
  nRounds?: number;
  context?: string;
  memberIds?: string[];
}

export interface DebateListQuery {
  status?: DebatePhase;
  page?: number;
  pageSize?: number;
}

export type DebatesListResponse = Debate[];

export interface ProviderInfo {
  id: string;
  name: string;
}

export interface ModelInfo {
  id: string;
  name: string;
}

export interface SortOptionInfo {
  value: string;
  label: string;
}


export interface ApiResponse<T> {
  data: T;
  metadata: {
    timestamp: string;
    version: string;
  };
}

export interface MemberStats {
  total: number;
  active: number;
  benched: number;
  retired: number;
}

export interface DebateStats {
  total: number;
  ready: number;
  completed: number;
  failed: number;
}

export interface DashboardStatsResponse {
  members: MemberStats;
  debates: DebateStats;
}
