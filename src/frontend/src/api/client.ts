const API_BASE = '/api';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!response.ok) {
    throw new Error(`API error: ${response.status} ${response.statusText}`);
  }
  if (response.status === 204) return undefined as T;
  return response.json();
}

export const api = {
  learners: {
    list: () => request<import('../types').Learner[]>('/learners'),
    get: (id: string) => request<import('../types').Learner>(`/learners/${id}`),
    create: (data: { name: string; role: string }) =>
      request<import('../types').Learner>('/learners', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: { name: string; role: string }) =>
      request<import('../types').Learner>(`/learners/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) =>
      request<void>(`/learners/${id}`, { method: 'DELETE' }),
  },

  sessions: {
    list: (params?: { learnerId?: string; type?: string; from?: string; to?: string }) => {
      const searchParams = new URLSearchParams();
      if (params?.learnerId) searchParams.set('learnerId', params.learnerId);
      if (params?.type) searchParams.set('type', params.type);
      if (params?.from) searchParams.set('from', params.from);
      if (params?.to) searchParams.set('to', params.to);
      const qs = searchParams.toString();
      return request<import('../types').TrainingSession[]>(`/sessions${qs ? `?${qs}` : ''}`);
    },
    get: (id: string) => request<import('../types').TrainingSession>(`/sessions/${id}`),
    start: (data: { learnerId: string; sessionType: string }) =>
      request<import('../types').TrainingSession>('/sessions', { method: 'POST', body: JSON.stringify(data) }),
    end: (id: string) =>
      request<import('../types').TrainingSession>(`/sessions/${id}/end`, { method: 'PUT' }),
    compressions: (id: string) =>
      request<import('../types').CompressionEvent[]>(`/sessions/${id}/compressions`),
    vitals: (id: string) =>
      request<import('../types').VitalSnapshot[]>(`/sessions/${id}/vitals`),
  },

  analytics: {
    learner: (id: string) =>
      request<import('../types').LearnerAnalytics>(`/analytics/learner/${id}`),
    session: (id: string) =>
      request<import('../types').SessionAnalytics>(`/analytics/session/${id}`),
  },

  scenarios: {
    list: () => request<import('../types').ScenarioInfo[]>('/scenarios'),
    activate: (data: { sessionId: string; scenarioName: string }) =>
      request<{ activated: boolean; scenarioName: string; transitionDurationMs: number }>(
        '/scenarios/activate',
        { method: 'POST', body: JSON.stringify(data) }
      ),
  },
};
