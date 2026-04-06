import {
    Pagination,
    PaginationContent,
    PaginationItem,
    PaginationNext,
    PaginationPrevious,
} from "@/shared/components/ui/pagination";
import { Field } from "@/shared/components/ui/field";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/components/ui/select";
interface PaginationProps {
    currentPage: number;
    totalPages: number;
    itemsPerPage: number;
    startIndex: number;
    endIndex: number;
    totalItems: number;
    totalDocs: number;
    onPageChange: (page: number) => void;
    onItemsPerPageChange: (value: number) => void;
}

export function CustomPagination({
    currentPage,
    totalPages,
    itemsPerPage,
    startIndex,
    endIndex,
    totalItems,
    totalDocs,
    onPageChange,
    onItemsPerPageChange,
}: PaginationProps) {
    const pageButtonClass =
        "min-w-9 h-9 px-3 rounded-xl text-sm font-medium transition-all duration-200";

    const renderPageButtons = () => {
        const maxPages = 3;
        const clampedStart = Math.max(
            1,
            Math.min(currentPage - 1, totalPages - (maxPages - 1)),
        );
        const visiblePageCount = Math.min(
            maxPages,
            totalPages - clampedStart + 1,
        );
        const visiblePages = Array.from(
            { length: visiblePageCount },
            (_, i) => clampedStart + i,
        );

        // If the visible pages include the first page, we can show them directly without an ellipsis
        if (clampedStart <= 1) {
            return visiblePages.map((pageNum) => (
                <PaginationItem key={pageNum}>
                    <button
                        onClick={() => onPageChange(pageNum)}
                        className={`${pageButtonClass} ${
                            currentPage === pageNum
                                ? "bg-[#007cfc] text-white shadow-sm"
                                : "border border-slate-300 bg-white text-slate-700 hover:border-[#007cfc]/50 hover:text-[#007cfc]"
                        }`}
                    >
                        {pageNum}
                    </button>
                </PaginationItem>
            ));
        }

        // Otherwise, we show the first page, an ellipsis, and the visible pages
        return (
            <>
                <PaginationItem key={1}>
                    <button
                        onClick={() => onPageChange(1)}
                        className={`${pageButtonClass} ${
                            currentPage === 1
                                ? "bg-[#007cfc] text-white shadow-sm"
                                : "border border-slate-300 bg-white text-slate-700 hover:border-[#007cfc]/50 hover:text-[#007cfc]"
                        }`}
                    >
                        1
                    </button>
                </PaginationItem>

                <PaginationItem key="ellipsis-leading">
                    <span className="px-1 text-sm font-medium text-slate-400 select-none">
                        ...
                    </span>
                </PaginationItem>

                {visiblePages.map((pageNum) => (
                    <PaginationItem key={pageNum}>
                        <button
                            onClick={() => onPageChange(pageNum)}
                            className={`${pageButtonClass} ${
                                currentPage === pageNum
                                    ? "bg-[#007cfc] text-white shadow-sm"
                                    : "border border-slate-300 bg-white text-slate-700 hover:border-[#007cfc]/50 hover:text-[#007cfc]"
                            }`}
                        >
                            {pageNum}
                        </button>
                    </PaginationItem>
                ))}
            </>
        );
    };

    return (
        <div className="mt-3 rounded-xl border border-slate-200 bg-slate-50/40 px-3 py-1 sm:px-3 sm:py-1 shadow-sm">
            {/* Left side: Info and items per page selector */}
            <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:gap-4">
                    <div className="text-sm text-slate-600 whitespace-nowrap">
                        <span className="hidden sm:inline">Hiển thị </span>
                        <span className="font-semibold text-slate-800">
                            {startIndex + 1}
                        </span>{" "}
                        -{" "}
                        <span className="font-semibold text-slate-800">
                            {Math.min(endIndex, totalItems)}
                        </span>{" "}
                        <span className="hidden xs:inline">trong </span>
                        <span className="xs:hidden">/ </span>
                        <span className="font-semibold text-slate-800">
                            {totalDocs}
                        </span>
                    </div>

                    <Field className="flex flex-row! items-center gap-2">
                        <Select
                            value={itemsPerPage.toString()}
                            onValueChange={(value) =>
                                onItemsPerPageChange(Number(value))
                            }
                        >
                            <SelectTrigger className="h-9 w-20 rounded-xl border-slate-300 bg-white text-sm hover:border-[#007cfc] focus:ring-2 focus:ring-[#007cfc]/30">
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="50">50</SelectItem>
                                <SelectItem value="100">100</SelectItem>
                                <SelectItem value="150">150</SelectItem>
                            </SelectContent>
                        </Select>
                        <span className="text-sm text-slate-600 whitespace-nowrap">
                            / trang
                        </span>
                    </Field>
                </div>

                {/* Right side: Pagination controls */}
                <Pagination className="mx-0 w-full lg:w-auto">
                    <PaginationContent className="flex-wrap justify-start gap-1.5 sm:justify-end sm:gap-2">
                        {/* Previous page */}
                        <PaginationItem>
                            <PaginationPrevious
                                onClick={() =>
                                    onPageChange(Math.max(1, currentPage - 1))
                                }
                                className={`h-9 rounded-xl border border-transparent px-3 text-sm font-medium transition-colors ${
                                    currentPage === 1
                                        ? "pointer-events-none opacity-45"
                                        : "cursor-pointer text-slate-700 hover:border-slate-300 hover:bg-white"
                                }`}
                                aria-disabled={currentPage === 1}
                            />
                        </PaginationItem>

                        {/* Page numbers */}
                        {renderPageButtons()}

                        {/* Next page */}
                        <PaginationItem>
                            <PaginationNext
                                onClick={() =>
                                    onPageChange(
                                        Math.min(totalPages, currentPage + 1),
                                    )
                                }
                                className={`h-9 rounded-xl border border-transparent px-3 text-sm font-medium transition-colors ${
                                    currentPage === totalPages
                                        ? "pointer-events-none opacity-45"
                                        : "cursor-pointer text-slate-700 hover:border-slate-300 hover:bg-white"
                                }`}
                                aria-disabled={currentPage === totalPages}
                            />
                        </PaginationItem>
                    </PaginationContent>
                </Pagination>
            </div>
        </div>
    );
}
