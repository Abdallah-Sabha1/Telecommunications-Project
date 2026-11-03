import { Skeleton } from "@/components/ui/skeleton";

export function TableSkeleton({ rows = 6, columns = 4 }: { rows?: number; columns?: number }) {
  return (
    <div aria-hidden="true" className="divide-y">
      {Array.from({ length: rows }, (_, rowIndex) => (
        <div
          className="grid items-center gap-4 px-4 py-3"
          key={rowIndex}
          style={{ gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))` }}
        >
          {Array.from({ length: columns }, (_, columnIndex) => (
            <Skeleton className="h-4 w-full max-w-[140px]" key={columnIndex} />
          ))}
        </div>
      ))}
    </div>
  );
}

export function CardSkeleton({ lines = 3 }: { lines?: number }) {
  return (
    <div aria-hidden="true" className="space-y-3 rounded-md border p-4">
      <Skeleton className="h-4 w-32" />
      {Array.from({ length: lines }, (_, index) => (
        <Skeleton className="h-3 w-full" key={index} />
      ))}
    </div>
  );
}

export function ChartSkeleton() {
  return (
    <div aria-hidden="true" className="rounded-md border p-4">
      <Skeleton className="h-4 w-28" />
      <Skeleton className="mt-4 h-[160px] w-full" />
    </div>
  );
}
