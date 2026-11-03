import type { ReactNode } from "react";
import { LayoutDashboard, Leaf, LogOut, TriangleAlert, Zap } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { cn } from "@/lib/utils";
import type { SessionUser } from "@/lib/types";

export type PageKey = "overview" | "incidents" | "energy" | "carbon";

const NAV: { key: PageKey; label: string; icon: typeof LayoutDashboard }[] = [
  { key: "overview", label: "Overview", icon: LayoutDashboard },
  { key: "incidents", label: "Incidents", icon: TriangleAlert },
  { key: "energy", label: "Energy & cost", icon: Zap },
  { key: "carbon", label: "Carbon report", icon: Leaf },
];

export function BrandMark({ className }: { className?: string }) {
  return (
    <span className={cn("flex w-full min-w-0 items-center gap-2.5 overflow-hidden", className)}>
      <img
        alt="Telecommunications Project"
        className="size-9 shrink-0 object-contain"
        src="/aithos-logo.png"
      />

      <span className="min-w-0 flex-1 truncate text-[13px] font-semibold tracking-tight text-foreground">
        Telecommunications Project
      </span>
    </span>
  );
}

export function AppShell({
  page,
  onNavigate,
  onSignOut,
  user,
  children,
}: {
  page: PageKey;
  onNavigate: (page: PageKey) => void;
  onSignOut: () => void;
  user: SessionUser;
  children: ReactNode;
}) {
  const initials = user.name.slice(0, 2).toUpperCase();

  return (
    <div className="min-h-dvh bg-background">
      <a
        className="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-50 focus:rounded-sm focus:bg-primary focus:px-3 focus:py-2 focus:text-sm focus:text-primary-foreground"
        href="#main-content"
      >
        Skip to main content
      </a>

      <div className="mx-auto flex min-h-dvh w-full max-w-[1440px]">
        <aside className="sticky top-0 hidden h-dvh w-64 shrink-0 flex-col border-r bg-sidebar lg:flex">
          <div className="flex h-16 items-center px-4">
            <BrandMark />
          </div>
          <Separator />
          <nav aria-label="Primary" className="flex flex-1 flex-col gap-0.5 p-3">
            <p className="px-2 pt-1 pb-2 text-xs font-medium tracking-wide text-muted-foreground uppercase">
              Workspace
            </p>
            {NAV.map((item) => {
              const active = item.key === page;
              return (
                <button
                  aria-current={active ? "page" : undefined}
                  className={cn(
                    "flex items-center gap-2.5 rounded-sm px-2 py-2 text-sm transition-colors",
                    active
                      ? "bg-sidebar-accent font-medium text-sidebar-accent-foreground"
                      : "text-muted-foreground hover:bg-sidebar-accent/60 hover:text-foreground",
                  )}
                  key={item.key}
                  onClick={() => onNavigate(item.key)}
                  type="button"
                >
                  <item.icon aria-hidden="true" className="size-4 shrink-0" />
                  {item.label}
                </button>
              );
            })}
          </nav>
          <Separator />
          <div className="flex items-center gap-3 p-3">
            <span
              aria-hidden="true"
              className="grid size-8 shrink-0 place-items-center rounded-sm bg-muted text-xs font-semibold text-foreground"
            >
              {initials}
            </span>
            <span className="min-w-0 flex-1">
              <span className="block truncate text-sm font-medium text-foreground">
                {user.name}
              </span>
              <span className="block truncate text-xs text-muted-foreground">Operations lead</span>
            </span>
            <Button aria-label="Sign out" onClick={onSignOut} size="icon" variant="ghost">
              <LogOut aria-hidden="true" className="size-4" />
            </Button>
          </div>
        </aside>

        <div className="flex min-w-0 flex-1 flex-col">
          <header className="sticky top-0 z-30 flex h-14 items-center gap-3 border-b bg-background/95 px-4 backdrop-blur-sm lg:hidden">
            <BrandMark />
            <nav aria-label="Primary" className="ml-auto flex items-center gap-1">
              {NAV.map((item) => (
                <Button
                  aria-current={item.key === page ? "page" : undefined}
                  key={item.key}
                  onClick={() => onNavigate(item.key)}
                  size="sm"
                  variant={item.key === page ? "secondary" : "ghost"}
                >
                  <item.icon aria-hidden="true" className="size-4" />
                  <span className="sr-only sm:not-sr-only">{item.label}</span>
                </Button>
              ))}
              <Button aria-label="Sign out" onClick={onSignOut} size="icon" variant="ghost">
                <LogOut aria-hidden="true" className="size-4" />
              </Button>
            </nav>
          </header>

          <main className="min-w-0 flex-1" id="main-content">
            {children}
          </main>
        </div>
      </div>
    </div>
  );
}
