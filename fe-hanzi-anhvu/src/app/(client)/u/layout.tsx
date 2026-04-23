"use client";
import { ClientDashboardHeader } from "@/shared/components/clients/nav.header";
import { ClientDashboardSidebar } from "@/shared/components/clients/slider";
import { useState } from "react";

const LayoutClient = ({ children }: { children: React.ReactNode }) => {
    const SIDEBAR_EXPANDED_WIDTH = 250;
    const SIDEBAR_COLLAPSED_WIDTH = 84;

    const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);

    const sidebarWidth = isSidebarCollapsed
        ? SIDEBAR_COLLAPSED_WIDTH
        : SIDEBAR_EXPANDED_WIDTH;

    return (
        <>
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
                    <main className="p-3">{children}</main>
                </div>
            </div>
        </>
    );
};

export default LayoutClient;
