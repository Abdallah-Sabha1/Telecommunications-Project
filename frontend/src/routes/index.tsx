import { useEffect, useState } from "react";
import { createFileRoute } from "@tanstack/react-router";

import { AppShell, type PageKey } from "@/components/app/app-shell";
import { CarbonReportPage } from "@/pages/carbon-report";
import { EnergyAnalyticsPage } from "@/pages/energy";
import { IncidentsPage } from "@/pages/incidents";
import { DashboardPage } from "@/pages/dashboard";
import { LoginPage } from "@/pages/login";
import { TowerDetailsPage } from "@/pages/tower-details";
import { fetchTowers } from "@/lib/api";
import type { SessionUser, Tower } from "@/lib/types";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "Telecommunications Project" },
      {
        name: "description",
        content: "Telecom tower health, energy, carbon and incident monitoring.",
      },
      { property: "og:title", content: "Telecommunications Project" },
      {
        property: "og:description",
        content: "Telecom tower health, energy, carbon and incident monitoring.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  component: App,
});

/**
 * Navigation remains state-based, exactly as in the original application.
 */
function App() {
  const [user, setUser] = useState<SessionUser | null>(null);
  const [page, setPage] = useState<PageKey>("overview");
  const [selectedTower, setSelectedTower] = useState<Tower | null>(null);
  const [returnPage, setReturnPage] = useState<PageKey>("overview");
  const [alertsVersion, setAlertsVersion] = useState(0);
  const [towers, setTowers] = useState<Tower[]>([]);

  useEffect(() => {
    if (!user) return;
    let alive = true;
    fetchTowers()
      .then((result) => {
        if (alive) setTowers(result.data);
      })
      .catch(() => undefined);
    return () => {
      alive = false;
    };
  }, [user]);

  if (!user) {
    return <LoginPage onLogin={setUser} />;
  }

  function handleSignOut() {
    setUser(null);
    setSelectedTower(null);
    setPage("overview");
  }

  function handleNavigate(next: PageKey) {
    setSelectedTower(null);
    setPage(next);
  }

  function openTower(tower: Tower, from: PageKey) {
    setReturnPage(from);
    setSelectedTower(tower);
  }

  return (
    <AppShell onNavigate={handleNavigate} onSignOut={handleSignOut} page={page} user={user}>
      {selectedTower ? (
        <TowerDetailsPage
          backLabel={returnPage === "incidents" ? "Incidents" : "Overview"}
          onAnalysisComplete={() => setAlertsVersion((version) => version + 1)}
          onBack={() => {
            setSelectedTower(null);
            setPage(returnPage);
          }}
          onSelectTower={setSelectedTower}
          tower={selectedTower}
          towers={towers.length ? towers : [selectedTower]}
        />
      ) : page === "carbon" ? (
        <CarbonReportPage onBack={() => setPage("overview")} />
      ) : page === "incidents" ? (
        <IncidentsPage
          onSelectTower={(tower) => openTower(tower, "incidents")}
          onOpenEnergy={() => setPage("energy")}
          towers={towers}
        />
      ) : page === "energy" ? (
        <EnergyAnalyticsPage onOpenCarbon={() => setPage("carbon")} />
      ) : (
        <DashboardPage
          alertsVersion={alertsVersion}
          onOpenIncidents={() => setPage("incidents")}
          onOpenCarbon={() => setPage("carbon")}
          onSelectTower={(tower) => openTower(tower, "overview")}
        />
      )}
    </AppShell>
  );
}
