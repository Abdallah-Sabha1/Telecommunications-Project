import {
  Area,
  AreaChart,
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import { AXIS, TOOLTIP_STYLE } from "@/components/app/chart-styles";

export type TrendPoint = Record<string, string | number>;

export function TrendChart({
  data,
  dataKey,
  title,
  unit,
  color,
  variant = "line",
}: {
  data: TrendPoint[];
  dataKey: string;
  title: string;
  unit: string;
  color: string;
  variant?: "line" | "area";
}) {
  const Chart = variant === "area" ? AreaChart : LineChart;

  return (
    <figure className="rounded-md border bg-card p-4">
      <figcaption className="flex items-baseline justify-between gap-2">
        <span className="text-sm font-medium text-foreground">{title}</span>
        <span className="text-xs text-muted-foreground">{unit}</span>
      </figcaption>
      <div className="mt-3">
        <ResponsiveContainer height={160} width="100%">
          <Chart data={data} margin={{ top: 4, right: 4, bottom: 0, left: 0 }}>
            <CartesianGrid stroke="var(--color-border)" strokeDasharray="3 3" vertical={false} />
            <XAxis axisLine={false} dataKey="time" minTickGap={24} tick={AXIS} tickLine={false} />
            <YAxis axisLine={false} tick={AXIS} tickLine={false} width={36} />
            <Tooltip contentStyle={TOOLTIP_STYLE} cursor={{ stroke: "var(--color-border)" }} />
            {variant === "area" ? (
              <Area
                dataKey={dataKey}
                fill={color}
                fillOpacity={0.12}
                stroke={color}
                strokeWidth={1.75}
                type="monotone"
              />
            ) : (
              <Line
                dataKey={dataKey}
                dot={false}
                stroke={color}
                strokeWidth={1.75}
                type="monotone"
              />
            )}
          </Chart>
        </ResponsiveContainer>
      </div>
    </figure>
  );
}
