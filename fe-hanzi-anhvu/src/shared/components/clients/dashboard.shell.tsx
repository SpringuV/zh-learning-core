"use client";

import { useState } from "react";
import { ClientDashboardMainContent } from "@/shared/components/clients/main.content";
import { ClientDashboardHeader } from "@/shared/components/clients/nav.header";
import { ClientDashboardSidebar } from "@/shared/components/clients/slider";

const SIDEBAR_EXPANDED_WIDTH = 250;
const SIDEBAR_COLLAPSED_WIDTH = 84;

export function ClientDashboardShell() {
    const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);

    const sidebarWidth = isSidebarCollapsed
        ? SIDEBAR_COLLAPSED_WIDTH
        : SIDEBAR_EXPANDED_WIDTH;

    return (
        <div className="flex min-h-screen flex-row bg-white text-slate-900">
            <div
                className="relative h-screen shrink-0 border-r border-slate-200 transition-[width] duration-200"
                style={{ width: `${sidebarWidth}px` }}
            >
                <ClientDashboardSidebar collapsed={isSidebarCollapsed} />
            </div>

            <div className="min-w-0 flex-1">
                <ClientDashboardHeader
                    isSidebarCollapsed={isSidebarCollapsed}
                    onToggleSidebar={() =>
                        setIsSidebarCollapsed((current) => !current)
                    }
                />
                <ClientDashboardMainContent />
            </div>
        </div>
    );
}
