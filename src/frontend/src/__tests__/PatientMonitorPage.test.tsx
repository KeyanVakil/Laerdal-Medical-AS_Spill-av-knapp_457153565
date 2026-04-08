import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import PatientMonitorPage from '../pages/PatientMonitor/PatientMonitorPage';

vi.mock('../api/client', () => ({
  api: {
    learners: {
      list: vi.fn().mockResolvedValue([
        { id: '1', name: 'Anna Olsen', role: 'Nurse', createdAt: '2024-01-01' },
      ]),
    },
    sessions: {
      start: vi.fn().mockResolvedValue({ id: 'session-1' }),
      end: vi.fn().mockResolvedValue({}),
    },
    scenarios: {
      activate: vi.fn().mockResolvedValue({ activated: true }),
    },
  },
}));

vi.mock('../api/signalr', () => ({
  monitorConnection: {
    on: vi.fn(),
    off: vi.fn(),
    invoke: vi.fn(),
  },
  startConnection: vi.fn().mockResolvedValue(undefined),
}));

vi.mock('recharts', () => ({
  LineChart: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
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

describe('PatientMonitorPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the page header', () => {
    renderWithRouter(<PatientMonitorPage />);
    expect(screen.getByText('Patient Monitor')).toBeInTheDocument();
  });

  it('renders all four vital displays', () => {
    renderWithRouter(<PatientMonitorPage />);
    expect(screen.getByText('Heart Rate')).toBeInTheDocument();
    expect(screen.getByText('SpO2')).toBeInTheDocument();
    expect(screen.getByText('Blood Pressure')).toBeInTheDocument();
    expect(screen.getByText('Respiratory Rate')).toBeInTheDocument();
  });

  it('renders Start Monitoring button', () => {
    renderWithRouter(<PatientMonitorPage />);
    expect(screen.getByText('Start Monitoring')).toBeInTheDocument();
  });

  it('shows dashes for initial vital values', () => {
    renderWithRouter(<PatientMonitorPage />);
    const dashes = screen.getAllByText('--');
    expect(dashes.length).toBeGreaterThanOrEqual(3);
  });
});
