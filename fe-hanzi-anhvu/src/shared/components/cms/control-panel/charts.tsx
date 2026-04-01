import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";
import {
    BarChart,
    Bar,
    LineChart,
    Line,
    PieChart,
    Pie,
    Cell,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer,
} from "recharts";

const userGrowthData = [
    { month: "Tháng 1", users: 400 },
    { month: "Tháng 2", users: 520 },
    { month: "Tháng 3", users: 680 },
    { month: "Tháng 4", users: 850 },
    { month: "Tháng 5", users: 1100 },
    { month: "Tháng 6", users: 1320 },
];

const revenueData = [
    { month: "Tháng 1", revenue: 2400 },
    { month: "Tháng 2", revenue: 3200 },
    { month: "Tháng 3", revenue: 4100 },
    { month: "Tháng 4", revenue: 5200 },
    { month: "Tháng 5", revenue: 6800 },
    { month: "Tháng 6", revenue: 8200 },
];

const topCoursesData = [
    { name: "HSK 1", students: 350 },
    { name: "HSK 2", students: 280 },
    { name: "HSK 3", students: 220 },
    { name: "HSK 4", students: 150 },
    { name: "HSK 5", students: 90 },
];

const COLORS = ["#0369a1", "#0ea5e9", "#06b6d4", "#14b8a6", "#10b981"];

export function UserGrowthChart() {
    return (
        <Card>
            <CardHeader>
                <CardTitle>Tăng Trưởng Người Dùng</CardTitle>
                <CardDescription>
                    Số lượng người dùng mới mỗi tháng
                </CardDescription>
            </CardHeader>
            <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={userGrowthData}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="month" />
                        <YAxis />
                        <Tooltip />
                        <Line
                            type="monotone"
                            dataKey="users"
                            stroke="#0369a1"
                            strokeWidth={2}
                            dot={{ fill: "#0369a1", r: 4 }}
                        />
                    </LineChart>
                </ResponsiveContainer>
            </CardContent>
        </Card>
    );
}

export function RevenueTrendChart() {
    return (
        <Card>
            <CardHeader>
                <CardTitle>Xu Hướng Doanh Thu</CardTitle>
                <CardDescription>
                    Doanh thu theo tháng (triệu đồng)
                </CardDescription>
            </CardHeader>
            <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={revenueData}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="month" />
                        <YAxis />
                        <Tooltip />
                        <Bar dataKey="revenue" fill="#10b981" />
                    </BarChart>
                </ResponsiveContainer>
            </CardContent>
        </Card>
    );
}

export function TopCoursesChart() {
    return (
        <Card>
            <CardHeader>
                <CardTitle>Khóa Học Phổ Biến</CardTitle>
                <CardDescription>Số lượng học sinh theo khóa</CardDescription>
            </CardHeader>
            <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={topCoursesData} layout="vertical">
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis type="number" />
                        <YAxis dataKey="name" type="category" width={60} />
                        <Tooltip />
                        <Bar dataKey="students" fill="#0ea5e9" />
                    </BarChart>
                </ResponsiveContainer>
            </CardContent>
        </Card>
    );
}

export function UserDistributionChart() {
    return (
        <Card>
            <CardHeader>
                <CardTitle>Phân Bố Người Dùng</CardTitle>
                <CardDescription>Theo level HSK</CardDescription>
            </CardHeader>
            <CardContent className="flex justify-center">
                <ResponsiveContainer width="100%" height={300}>
                    <PieChart>
                        <Pie
                            data={topCoursesData}
                            cx="50%"
                            cy="50%"
                            labelLine={false}
                            label={(entry: any) =>
                                `${entry.name} (${entry.students})`
                            }
                            outerRadius={80}
                            fill="#8884d8"
                            dataKey="students"
                        >
                            {topCoursesData.map((entry, index) => (
                                <Cell
                                    key={`cell-${index}`}
                                    fill={COLORS[index % COLORS.length]}
                                />
                            ))}
                        </Pie>
                        <Tooltip />
                    </PieChart>
                </ResponsiveContainer>
            </CardContent>
        </Card>
    );
}
