"use client";

import { AppSidebar } from "@/shared/components/cms/app-sidebar";
import {
    RevenueTrendChart,
    TopCoursesChart,
    UserDistributionChart,
    UserGrowthChart,
} from "@/shared/components/cms/control-panel/charts";
import { DashboardHeader } from "@/shared/components/cms/control-panel/header";
import { KPICards } from "@/shared/components/cms/control-panel/kpi-cards";
import {
    RecentEnrollmentsTable,
    RecentPaymentsTable,
} from "@/shared/components/cms/control-panel/tables";
import Page403 from "@/shared/components/errors/page403";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbList,
    BreadcrumbPage,
} from "@/shared/components/ui/breadcrumb";
import { Separator } from "@/shared/components/ui/separator";
import {
    SidebarInset,
    SidebarProvider,
    SidebarTrigger,
} from "@/shared/components/ui/sidebar";
import { useSession } from "next-auth/react";

export default function Page() {
    const { data: session, status } = useSession();

    if (status === "loading") {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="text-muted-foreground">
                    Đang kiểm tra phiên làm việc...
                </div>
            </div>
        );
    }

    // Check role and permission
    if (!session?.user?.roles?.includes("Administrators")) {
        // Redirect to 403 page or handle unauthorized access
        return <Page403 />;
    }

    return (
        <SidebarProvider>
            <AppSidebar />
            <SidebarInset>
                <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
                    <div className="flex items-center gap-2 px-4">
                        <SidebarTrigger className="-ml-1" />
                        <Separator
                            orientation="vertical"
                            className="mr-2 data-vertical:h-4 data-vertical:self-auto"
                        />
                        <Breadcrumb>
                            <BreadcrumbList>
                                <BreadcrumbItem>
                                    <BreadcrumbPage>
                                        Bảng Điều Khiển
                                    </BreadcrumbPage>
                                </BreadcrumbItem>
                            </BreadcrumbList>
                        </Breadcrumb>
                    </div>
                </header>

                <div className="flex flex-1 flex-col gap-6 p-6 pt-0">
                    {/* Header */}
                    <DashboardHeader />

                    {/* KPI Cards */}
                    <KPICards />

                    {/* Charts */}
                    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                        <UserGrowthChart />
                        <RevenueTrendChart />
                        <TopCoursesChart />
                        <UserDistributionChart />
                    </div>

                    {/* Tables */}
                    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                        <RecentEnrollmentsTable />
                        <RecentPaymentsTable />
                    </div>
                </div>
            </SidebarInset>
        </SidebarProvider>
    );
}
