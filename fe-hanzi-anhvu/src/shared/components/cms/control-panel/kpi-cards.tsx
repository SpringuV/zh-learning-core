import {
    Card,
    CardContent,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";
import { Users, TrendingUp, BookOpen, ShoppingCart } from "lucide-react";

export function KPICards() {
    return (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
            <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">
                        Tổng Người Dùng
                    </CardTitle>
                    <Users className="size-4 text-blue-600" />
                </CardHeader>
                <CardContent>
                    <div className="text-2xl font-bold">1,320</div>
                    <p className="text-xs text-slate-600">
                        <span className="text-green-600 font-semibold">
                            +20%
                        </span>{" "}
                        so với tháng trước
                    </p>
                </CardContent>
            </Card>

            <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">
                        Tổng Doanh Thu
                    </CardTitle>
                    <TrendingUp className="size-4 text-green-600" />
                </CardHeader>
                <CardContent>
                    <div className="text-2xl font-bold">82.2M₫</div>
                    <p className="text-xs text-slate-600">
                        <span className="text-green-600 font-semibold">
                            +15%
                        </span>{" "}
                        so với tháng trước
                    </p>
                </CardContent>
            </Card>

            <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">
                        Khóa Học Đang Hoạt Động
                    </CardTitle>
                    <BookOpen className="size-4 text-orange-600" />
                </CardHeader>
                <CardContent>
                    <div className="text-2xl font-bold">12</div>
                    <p className="text-xs text-slate-600">
                        <span className="text-blue-600 font-semibold">
                            2 khóa
                        </span>{" "}
                        mới tháng này
                    </p>
                </CardContent>
            </Card>

            <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">
                        Tổng Ghi Danh
                    </CardTitle>
                    <ShoppingCart className="size-4 text-purple-600" />
                </CardHeader>
                <CardContent>
                    <div className="text-2xl font-bold">1,850</div>
                    <p className="text-xs text-slate-600">
                        <span className="text-green-600 font-semibold">
                            +12%
                        </span>{" "}
                        so với tháng trước
                    </p>
                </CardContent>
            </Card>
        </div>
    );
}
