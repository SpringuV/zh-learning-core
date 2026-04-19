import { ArrowRight, Sparkles, Users } from "lucide-react";

const courseItems = [
    {
        title: "DSA #1: Nền tảng - Complexity, Toán học & Bit Manipulation",
        description:
            "Phần 1/13 trong lộ trình DSA. Bao gồm lý thuyết chi tiết, code Python, mô phỏng từng bước và bài LeetCode.",
        lessons: "5 bài học",
        students: "1386 học viên",
    },
    {
        title: "Nhập môn lập trình Python",
        description:
            "Khóa học lập trình Python dành cho người chưa biết gì. Học mà như không học, mỗi khái niệm được giải thích qua ví dụ trực quan.",
        lessons: "47 bài học",
        students: "480 học viên",
    },
    {
        title: "Nền Tảng Database",
        description:
            "5 câu hỏi phỏng vấn nền tảng về Database: DBMS Languages, ACID, Normalization, ER Diagram và SQL Joins.",
        lessons: "5 bài học",
        students: "395 học viên",
    },
    {
        title: "DSA #2: Arrays & Strings",
        description:
            "Phần 2/13 trong lộ trình DSA. Bao gồm lý thuyết chi tiết, code Python, mô phỏng từng bước và bài LeetCode.",
        lessons: "10 bài học",
        students: "273 học viên",
    },
    {
        title: "Làm quen với Git",
        description:
            "Khóa học đầu tiên giúp bạn hiểu Git là gì, tại sao cần dùng, cách cài đặt GitHub Desktop, tạo repository và commit thay đổi.",
        lessons: "21 bài học",
        students: "220 học viên",
    },
    {
        title: "DSA #3: Searching & Sorting",
        description:
            "Phần 3/13 trong lộ trình DSA. Bao gồm lý thuyết chi tiết, code Python, mô phỏng từng bước và bài LeetCode.",
        lessons: "9 bài học",
        students: "194 học viên",
    },
];

export function ClientDashboardMainContent() {
    return (
        <section className="min-h-0 flex-1 w-full overflow-y-auto px-4 py-6 md:px-6">
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
                {courseItems.map((item, index) => (
                    <div
                        key={index}
                        className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition-shadow hover:shadow-md"
                    >
                        <h3 className="font-semibold text-slate-800">
                            {item.title}
                        </h3>
                        <p className="mt-2 text-sm text-slate-600">
                            {item.description}
                        </p>
                        <div className="mt-4 flex items-center justify-between">
                            <span className="text-xs font-medium text-slate-500">
                                {item.lessons}
                            </span>
                            <span className="text-xs font-medium text-slate-500">
                                {item.students}
                            </span>
                        </div>
                    </div>
                ))}
            </div>
        </section>
    );
}
