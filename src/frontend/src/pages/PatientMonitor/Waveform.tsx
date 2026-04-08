import { LineChart, Line, ResponsiveContainer } from 'recharts';

interface WaveformProps {
  data: number[];
  color: string;
  height?: number;
}

export default function Waveform({ data, color, height = 60 }: WaveformProps) {
  const chartData = data.map((v, i) => ({ i, v }));

  return (
    <div style={{ height }}>
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={chartData}>
          <Line
            type="monotone"
            dataKey="v"
            stroke={color}
            strokeWidth={1.5}
            dot={false}
            isAnimationActive={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
