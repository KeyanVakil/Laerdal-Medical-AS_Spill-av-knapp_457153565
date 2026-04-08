import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import CprTrainingPage from '../pages/CprTraining/CprTrainingPage';

vi.mock('../api/client', () => ({
  api: {
    learners: {
      list: vi.fn().mockResolvedValue([
        { id: '1', name: 'Anna Olsen', role: 'Nurse', createdAt: '2024-01-01' },
      ]),
    },
    sessions: {
      start: vi.fn().mockResolvedValue({ id: 'session-1', learnerId: '1', sessionType: 'CprTraining', startedAt: new Date().toISOString() }),
      end: vi.fn().mockResolvedValue({}),
    },
  },
}));

vi.mock('../api/signalr', () => ({
  cprConnection: {
    on: vi.fn(),
    off: vi.fn(),
    invoke: vi.fn(),
  },
  startConnection: vi.fn().mockResolvedValue(undefined),
}));

vi.mock('recharts', () => ({
  LineChart: ({ children }: { children: React.ReactNode }) => <div data-testid="line-chart">{children}</div>,
  Line: () => <div />,
  XAxis: () => <div />,
  YAxis: () => <div />,
  CartesianGrid: () => <div />,
  Tooltip: () => <div />,
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));

function renderWithRouter(ui: React.ReactElement) {
  return render(<BrowserRouter>{ui}</BrowserRouter>);
}

describe('CprTrainingPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the page header', () => {
    renderWithRouter(<CprTrainingPage />);
    expect(screen.getByText('CPR Training')).toBeInTheDocument();
  });

  it('renders learner selector', () => {
    renderWithRouter(<CprTrainingPage />);
    expect(screen.getByText('Select a learner...')).toBeInTheDocument();
  });

  it('renders Start Session button', () => {
    renderWithRouter(<CprTrainingPage />);
    expect(screen.getByText('Start Session')).toBeInTheDocument();
  });

  it('disables Start Session when no learner selected', () => {
    renderWithRouter(<CprTrainingPage />);
    const btn = screen.getByText('Start Session');
    expect(btn).toBeDisabled();
  });

  it('shows waiting text initially', () => {
    renderWithRouter(<CprTrainingPage />);
    expect(screen.getByText('Waiting for compression data...')).toBeInTheDocument();
  });
});
