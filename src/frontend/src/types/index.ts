export interface Learner {
  id: string;
  name: string;
  role: string;
  createdAt: string;
  sessionCount?: number;
  averageScore?: number;
}

export type SessionType = 'CprTraining' | 'PatientMonitoring';

export interface TrainingSession {
  id: string;
  learnerId: string;
  learnerName?: string;
  sessionType: SessionType;
  startedAt: string;
  endedAt?: string;
  overallScore?: number;
}

export interface CompressionEvent {
  id: number;
  timestamp: string;
  depthCm: number;
  rateBpm: number;
  fullRecoil: boolean;
  qualityScore: number;
  depthFeedback?: string;
  rateFeedback?: string;
  sessionId?: string;
}

export interface VitalSnapshot {
  sessionId?: string;
  timestamp: string;
  heartRate: number;
  spO2: number;
  systolicBp: number;
  diastolicBp: number;
  respiratoryRate: number;
}

export interface ScenarioInfo {
  name: string;
  vitals: {
    heartRate: string;
    spO2: string;
    bloodPressure: string;
    respiratoryRate: string;
  };
}

export interface LearnerAnalytics {
  learnerId: string;
  totalSessions: number;
  averageScore: number;
  bestScore: number;
  recentTrend: string;
  sessionScores: { id: string; date: string; score: number }[];
  suggestions: string[];
}

export interface SessionAnalytics {
  sessionId: string;
  startedAt: string;
  endedAt?: string;
  overallScore?: number;
  totalCompressions: number;
  averageDepth: number;
  averageRate: number;
  recoilRate: number;
  depthDistribution: { below5: number; optimal: number; above6: number };
  rateDistribution: { below100: number; optimal: number; above120: number };
  compressions: CompressionEvent[];
  suggestions: string[];
}
