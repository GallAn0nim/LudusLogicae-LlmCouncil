import type {
  Member,
  MemberListQuery,
  MembersListResponse,
  CreateMemberRequest,
  UpdateMemberRequest,
  MemberIntroductionResponse,
  Debate,
  DebateListQuery,
  DebatesListResponse,
  CreateDebateRequest,
  UpdateDebateRequest,
  ProviderInfo,
  ModelInfo,
  SortOptionInfo,
  ApiResponse,
  DashboardStatsResponse,
} from '../types';

const API_BASE = '/api';

class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
    this.name = 'ApiError';
  }
}

async function request<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const url = `${API_BASE}${endpoint}`;

  const config: RequestInit = {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  };

  const response = await fetch(url, config);

  if (!response.ok) {
    const errorText = await response.text();
    throw new ApiError(response.status, errorText || `HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const json = await response.json();

  if (json.response?.data !== undefined) {
    return json.response.data;
  }

  return json.data !== undefined ? json.data : json;
}

function buildQueryString(params: object): string {
  const searchParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      searchParams.append(key, String(value));
    }
  });

  const queryString = searchParams.toString();
  return queryString ? `?${queryString}` : '';
}

export const membersApi = {
  list: (query: MemberListQuery = {}): Promise<MembersListResponse> => {
    return request(`/members${buildQueryString(query)}`);
  },

  get: (id: string): Promise<Member> => {
    return request(`/members/${id}`);
  },

  create: (data: CreateMemberRequest): Promise<Member> => {
    return request('/members', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: (id: string, data: UpdateMemberRequest): Promise<Member> => {
    return request(`/members/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  delete: (id: string): Promise<void> => {
    return request(`/members/${id}`, {
      method: 'DELETE',
    });
  },

  getIntroduction: (id: string): Promise<MemberIntroductionResponse> => {
    return request(`/members/${id}/introductions`, {
      method: 'POST',
    });
  },
};

export const debatesApi = {
  list: (query: DebateListQuery = {}): Promise<DebatesListResponse> => {
    return request(`/debates${buildQueryString(query)}`);
  },

  get: (id: string): Promise<Debate> => {
    return request(`/debates/${id}`);
  },

  create: (data: CreateDebateRequest): Promise<Debate> => {
    return request('/debates', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: (id: string, data: UpdateDebateRequest): Promise<Debate> => {
    return request(`/debates/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  delete: (id: string): Promise<void> => {
    return request(`/debates/${id}`, {
      method: 'DELETE',
    });
  },

  run: (id: string): Promise<Debate> => {
    return request(`/debates/${id}/fully-run`, {
      method: 'POST',
    });
  },
};

export const dashboardApi = {
  getStats: (): Promise<DashboardStatsResponse> => {
    return request('/dashboard/stats');
  },
};

export const metadataApi = {
  ping: (): Promise<string> => {
    return request('/metadata/ping');
  },

  getProviders: (): Promise<ProviderInfo[]> => {
    return request('/metadata/dictionary/provider');
  },

  getModels: (providerId: string): Promise<ModelInfo[]> => {
    return request(`/metadata/dictionary/provider/${providerId}/models`);
  },

  getDebatePhases: (): Promise<string[]> => {
    return request('/metadata/dictionary/debate-phase');
  },

  getMemberSortOptions: (): Promise<SortOptionInfo[]> => {
    return request('/metadata/dictionary/member-sort-options');
  },
};

export { ApiError };
