import { Metadata } from "next";
import ResultClientComponent from "@/modules/lesson/components/result/client.session.result";

export const metadata: Metadata = {
    title: "Kết quả phiên học - HanziAnhVu",
};

const TopicExerciseResultPage = () => {
    return <ResultClientComponent />;
};

export default TopicExerciseResultPage;
