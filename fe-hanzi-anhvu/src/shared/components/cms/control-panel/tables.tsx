import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";

const recentEnrollments = [
    {
        id: 1,
        userName: "Nguyễn Văn A",
        course: "HSK 1",
        date: "2024-03-28",
        status: "Hoạt động",
    },
    {
        id: 2,
        userName: "Trần Thị B",
        course: "HSK 2",
        date: "2024-03-27",
        status: "Hoạt động",
    },
    {
        id: 3,
        userName: "Lê Văn C",
        course: "HSK 3",
        date: "2024-03-26",
        status: "Chưa bắt đầu",
    },
    {
        id: 4,
        userName: "Phạm Thị D",
        course: "HSK 1",
        date: "2024-03-25",
        status: "Hoạt động",
    },
    {
        id: 5,
        userName: "Hoàng Văn E",
        course: "HSK 4",
        date: "2024-03-24",
        status: "Hoạt động",
    },
];

const recentPayments = [
    {
        id: 1,
        userName: "Nguyễn Văn A",
        amount: 299000,
        course: "HSK 1",
        date: "2024-03-28",
        status: "Thành công",
    },
    {
        id: 2,
        userName: "Trần Thị B",
        amount: 399000,
        course: "HSK 2",
        date: "2024-03-27",
        status: "Thành công",
    },
    {
        id: 3,
        userName: "Lê Văn C",
        amount: 499000,
        course: "HSK 3",
        date: "2024-03-26",
        status: "Đang xử lý",
    },
    {
        id: 4,
        userName: "Phạm Thị D",
        amount: 299000,
        course: "HSK 1",
        date: "2024-03-25",
        status: "Thành công",
    },
];

export function RecentEnrollmentsTable() {
    return (
        <Card>
            <CardHeader>
                <CardTitle>Ghi Danh Gần Đây</CardTitle>
                <CardDescription>5 ghi danh mới nhất</CardDescription>
            </CardHeader>
            <CardContent>
                <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                        <thead>
                            <tr className="border-b">
                                <th className="text-left py-2">Người Dùng</th>
                                <th className="text-left py-2">Khóa</th>
                                <th className="text-left py-2">Ngày</th>
                                <th className="text-left py-2">Trạng Thái</th>
                            </tr>
                        </thead>
                        <tbody>
                            {recentEnrollments.map((item) => (
                                <tr key={item.id} className="border-b">
                                    <td className="py-2">{item.userName}</td>
                                    <td className="py-2">{item.course}</td>
                                    <td className="py-2 text-slate-600">
                                        {item.date}
                                    </td>
                                    <td className="py-2">
                                        <span
                                            className={`px-2 py-1 rounded text-xs font-medium ${
                                                item.status === "Hoạt động"
                                                    ? "bg-green-100 text-green-800"
                                                    : "bg-yellow-100 text-yellow-800"
                                            }`}
                                        >
                                            {item.status}
                                        </span>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </CardContent>
        </Card>
    );
}

export function RecentPaymentsTable() {
    return (
        <Card>
            <CardHeader>
                <CardTitle>Thanh Toán Gần Đây</CardTitle>
                <CardDescription>4 thanh toán mới nhất</CardDescription>
            </CardHeader>
            <CardContent>
                <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                        <thead>
                            <tr className="border-b">
                                <th className="text-left py-2">Người Dùng</th>
                                <th className="text-left py-2">Số Tiền</th>
                                <th className="text-left py-2">Ngày</th>
                                <th className="text-left py-2">Trạng Thái</th>
                            </tr>
                        </thead>
                        <tbody>
                            {recentPayments.map((item) => (
                                <tr key={item.id} className="border-b">
                                    <td className="py-2">{item.userName}</td>
                                    <td className="py-2 font-semibold">
                                        {item.amount.toLocaleString()}₫
                                    </td>
                                    <td className="py-2 text-slate-600">
                                        {item.date}
                                    </td>
                                    <td className="py-2">
                                        <span
                                            className={`px-2 py-1 rounded text-xs font-medium ${
                                                item.status === "Thành công"
                                                    ? "bg-green-100 text-green-800"
                                                    : "bg-blue-100 text-blue-800"
                                            }`}
                                        >
                                            {item.status}
                                        </span>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </CardContent>
        </Card>
    );
}
