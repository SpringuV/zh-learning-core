import { Button } from "@/shared/components/ui/button";
import { Plus } from "lucide-react";

export function DashboardHeader() {
    return (
        <div className="flex items-center justify-between">
            <div>
                <h1 className="text-3xl font-bold">Bảng Điều Khiển</h1>
                <p className="text-slate-600">
                    Tổng quan tình hình hoạt động của HanZi
                </p>
            </div>
            <Button className="gap-2">
                <Plus className="size-4" />
                Tạo Khóa Học
            </Button>
        </div>
    );
}
