import { BookOpen, Gem, Map, Route, ChevronRight, History } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";

type ClientDashboardSidebarProps = {
    collapsed?: boolean;
};

export function ClientDashboardSidebar({
    collapsed = false,
}: ClientDashboardSidebarProps) {
    const path = usePathname();
    const learningItems = [
        // mục ôn tập, mục này sẽ hiển thị các bài học mà học viên đánh dấu là cần ôn tập thêm vào đây
        {
            label: "Trang chủ",
            icon: Gem,
            active: path === "/u/dashboard",
            href: "/u/dashboard",
        },
        {
            label: "Ôn tập",
            icon: History,
            active: path === "/u/review",
            href: "/u/review",
        },
        {
            label: "Khóa học của tôi",
            icon: BookOpen,
            active: path === "/u/my-courses",
            href: "/u/my-courses",
        },
        {
            label: "Lộ trình học",
            icon: Route,
            active: path === "/u/learning-path",
            href: "/u/learning-path",
        },
        {
            label: "Roadmap",
            icon: Map,
            active: path === "/u/roadmap",
            href: "/u/roadmap",
        },
    ];
    return (
        <aside
            className={
                "h-full w-full overflow-y-auto bg-white py-5 transition-all " +
                (collapsed ? "px-2" : "px-4")
            }
        >
            <div
                className={
                    "rounded-2xl " +
                    (collapsed
                        ? "p-2"
                        : "p-3 border border-slate-200 bg-slate-50 ")
                }
            >
                <div
                    className={
                        "flex items-center " +
                        (collapsed ? "justify-center" : "gap-3")
                    }
                >
                    <Link
                        href="/u/dashboard"
                        className="flex items-center justify-center gap-2"
                    >
                        <div className="relative">
                            {collapsed ? (
                                <div className="grid size-9 place-items-center rounded-lg bg-emerald-100 text-xs font-black text-emerald-700">
                                    HV
                                </div>
                            ) : (
                                <p className="text-xl font-black leading-none tracking-tight text-slate-900">
                                    HanziAnhVu
                                </p>
                            )}
                        </div>
                    </Link>
                </div>
            </div>

            {!collapsed ? (
                <div className="mt-5 px-2 text-[11px] font-bold uppercase tracking-[0.2em] text-slate-500">
                    Học tập
                </div>
            ) : null}

            <nav className="mt-2 space-y-1">
                {learningItems.map((item) => {
                    const Icon = item.icon;
                    return (
                        <Link
                            key={item.label}
                            href={item.href}
                            className={
                                "flex w-full items-center rounded-xl py-2.5 transition-colors " +
                                (item.active
                                    ? "bg-emerald-100 text-emerald-700"
                                    : "text-slate-600 hover:bg-slate-100 hover:text-slate-900") +
                                (collapsed
                                    ? " justify-center px-2"
                                    : " justify-between px-3 text-left")
                            }
                            title={item.label}
                        >
                            <span
                                className={
                                    "flex items-center text-[15px] font-semibold " +
                                    (collapsed ? "gap-0" : "gap-3")
                                }
                            >
                                <Icon className="size-4" />
                                {!collapsed ? item.label : null}
                            </span>
                            {/* {!collapsed ? (
                                <span className="flex items-center gap-2">
                                    {item.active ? (
                                        <ChevronRight className="size-4" />
                                    ) : null}
                                </span>
                            ) : null} */}
                        </Link>
                    );
                })}
            </nav>

            <div className="my-4 h-px bg-slate-200" />

            <div
                className={
                    "mt-5 rounded-xl border border-amber-300/50 bg-amber-50 text-[11px] text-slate-700 " +
                    (collapsed ? "p-2" : "p-3")
                }
            >
                <div
                    className={
                        "flex items-center " +
                        (collapsed ? "justify-center" : "gap-2")
                    }
                >
                    <Gem className="size-4 text-amber-600" />
                    {!collapsed ? "Bạn đang ở gói miễn phí." : null}
                </div>
            </div>
        </aside>
    );
}
