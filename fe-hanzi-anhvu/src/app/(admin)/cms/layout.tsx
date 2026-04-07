import { auth } from "@/auth";
import { AppSidebar } from "@/shared/components/cms/app-sidebar";
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
import { headers } from "next/headers";
export default async function CmsLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    const session = await auth();
    console.log("[CMS Layout] Session:", session);
    if (!session?.user?.id) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="text-muted-foreground">
                    Bạn cần đăng nhập để truy cập CMS.
                </div>
            </div>
        );
    }

    // Check role and permission
    if (!session?.user?.roles?.includes("Administrators")) {
        // Redirect to 403 page or handle unauthorized access
        return <Page403 />;
    }
    const route = {
        userManagement: {
            list: {
                url: "/cms/users",
                title: "Danh Sách User",
            },
        },
        dashboard: {
            main: {
                url: "/cms/dashboard",
                title: "Bảng Điều Khiển",
            },
        },
        lessons: {
            list: {
                url: "/cms/lessons",
                title: "Danh Sách Bài Học",
            },
        },
        categories: {
            list: {
                url: "/cms/categories",
                title: "Danh Sách Danh Mục",
            },
        },
    };
    // Determine current route to set breadcrumb title. This is a simple implementation,
    // you can enhance it based on your routing structure.
    const headerStore = await headers();
    const pathName = headerStore.get("x-pathname") ?? "/cms/dashboard";
    const currentRoute =
        Object.values(route)
            .map((group) => Object.values(group))
            .flat()
            .find((item) => pathName.startsWith(item.url))?.title ??
        route.dashboard.main.title;

    console.log("[CMS Layout] Current Pathname:", pathName);
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
                                        {currentRoute}
                                    </BreadcrumbPage>
                                </BreadcrumbItem>
                            </BreadcrumbList>
                        </Breadcrumb>
                    </div>

                    {/* Bổ sung notify */}
                </header>

                {/* content */}
                <div className="flex flex-1 flex-col gap-6 p-6 pt-0">
                    {children}
                </div>
            </SidebarInset>
        </SidebarProvider>
    );
}
