"use client";
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

export default function Page() {
    return (
        <div className="flex flex-1 flex-col gap-6">
            <DashboardHeader />

            <KPICards />

            <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                <UserGrowthChart />
                <RevenueTrendChart />
                <TopCoursesChart />
                <UserDistributionChart />
            </div>

            <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                <RecentEnrollmentsTable />
                <RecentPaymentsTable />
            </div>
        </div>
    );
}
