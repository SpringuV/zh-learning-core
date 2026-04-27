import {
    CountinueLearningSessionResponse,
    CourseTopicsOverviewResponse,
    StartLearningTopicResponse,
    TopicClientDashboardItemResponse,
    TopicCreateRequest,
    TopicCreateResponseData,
    TopicDetailResponse,
    TopicListItemAdmin,
    TopicQueryParams,
    TopicReOrderRequest,
    UpdateTopicRequest,
} from "@/modules/lesson/types/topic.type";
import {
    AdminBaseListResponse,
    BaseResponse,
    sanitizeQueryParams,
} from "@/shared/types/store.type";
import http from "@/shared/utils/http";

// #region API Endpoints
const endPoints = {
    startLearning: (slug: string) =>
        `lesson/v1/topic-progress/exercise-session/${slug}/started`,
    createTopic: "lesson/v1/topic",
    listTopics: "search/v1/topics",
    courseTopics: "search/v1/course-topics",
    updateTopic: `lesson/v1/topic`,
    publishTopic: (topicId: string) => `lesson/v1/topic/${topicId}/publish`,
    unPublishTopic: (topicId: string) => `lesson/v1/topic/${topicId}/unpublish`,
    reOrderTopic: "lesson/v1/topic/reorder",
    deleteTopic: (topicId: string) => `lesson/v1/topic/${topicId}`,
    detailTopic: (topicId: string) => `search/v1/topics/${topicId}`,
    topicsForClient: (slug: string) => `search/v1/topics/dashboard/${slug}`,
    countinueLearningSessionForTopicClient: (slug: string) =>
        `search/v1/topics/${slug}/continue-learning-session`,
};
// #endregion

export const topicApi = {
    async createTopic(payload: TopicCreateRequest) {
        return await http.post<TopicCreateResponseData>(
            endPoints.createTopic,
            payload,
        );
    },
    async getListTopics(
        courseId: string,
        queryParams: Partial<TopicQueryParams>,
    ) {
        return await http.get<AdminBaseListResponse<TopicListItemAdmin>>(
            endPoints.listTopics,
            {
                params: sanitizeQueryParams({
                    courseId,
                    ...queryParams,
                }),
            },
        );
    },
    async getCourseTopicsOverview(
        courseId: string,
        queryParams: Partial<TopicQueryParams>,
    ) {
        return await http.get<CourseTopicsOverviewResponse>(
            endPoints.courseTopics,
            {
                params: sanitizeQueryParams({
                    courseId,
                    ...queryParams,
                }),
            },
        );
    },
    async publishTopic(topicId: string) {
        return await http.post(endPoints.publishTopic(topicId));
    },
    async unPublishTopic(topicId: string) {
        return await http.post(endPoints.unPublishTopic(topicId));
    },
    async reOrderTopic(payload: TopicReOrderRequest) {
        return await http.post(endPoints.reOrderTopic, payload);
    },
    async updateTopic(payload: UpdateTopicRequest) {
        return await http.patch(endPoints.updateTopic, payload);
    },
    async deleteTopic(topicId: string) {
        return await http.delete(endPoints.deleteTopic(topicId));
    },
    async getTopicDetail(topicId: string) {
        return await http.get<TopicDetailResponse>(
            endPoints.detailTopic(topicId),
        );
    },
    async getTopicsForClient(slug: string) {
        return await http.get<BaseResponse<TopicClientDashboardItemResponse[]>>(
            endPoints.topicsForClient(slug),
        );
    },
    async startLearningTopic(slugTopic: string) {
        return await http.post<BaseResponse<StartLearningTopicResponse>>(
            endPoints.startLearning(slugTopic),
        );
    },
    async continueLearningSessionForTopicClient(slug: string) {
        return await http.get<BaseResponse<CountinueLearningSessionResponse>>(
            endPoints.countinueLearningSessionForTopicClient(slug),
        );
    },
};
// #endregion
