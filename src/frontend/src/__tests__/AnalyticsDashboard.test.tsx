import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import AnalyticsDashboard from '../pages/Analytics/AnalyticsDashboard';

vi.mock('../api/client', () => ({
  api: {
    learners: {
      list: vi.fn().mockResolvedValue([
        { id: '1', name: 'Anna Olsen', role: 'Nurse', createdAt: '2024-01-01' },
      ]),
    },
    sessions: {
      list: vi.fn().mockResolvedValue([]),
    },
    analytics: {
      learner: vi.fn().mockResolvedValue({
        learnerId: '1',
        totalSessions: 5,
        averageScore: 78.5,
        bestScore: 92.0,
        recentTrend: 'improving',
        sessionScores: [],
        suggestions: ['Keep practicing'],
      }),
      session: vi.fn().mockResolvedValue({
        sessionId: 'session-1',
        totalCompressions: 100,
        averageDepth: 5.3,
        averageRate: 112,
        recoilRate: 85,
        depthDistribution: { below5: 10, optimal: 80, above6: 10 },
        rateDistribution: { below100: 5, optimal: 90, above120: 5 },
        compressions: [],
        suggestions: [],
      }),
    },
  },
}));

vi.mock('recharts', () => ({
  LineChart: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  Line: () => <div />,
  BarChart: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  Bar: () => <div />,
  XAxis: () => <div />,
  YAxis: () => <div />,
  CartesianGrid: () => <div />,
  Tooltip: () => <div />,
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));

function renderWithRouter(ui: React.ReactElement) {
  return render(<BrowserRouter>{ui}</BrowserRouter>);
}

describe('AnalyticsDashboard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the page header', () => {
    renderWithRouter(<AnalyticsDashboard />);
    expect(screen.getByText('Training Analytics')).toBeInTheDocument();
  });

  it('renders learner filter', () => {
    renderWithRouter(<AnalyticsDashboard />);
    expect(screen.getByText('Select a learner...')).toBeInTheDocument();
  });
});
