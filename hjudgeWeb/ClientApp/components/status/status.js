import { setTitle } from '../../utilities/titleHelper';
import { Get } from '../../utilities/requestHelper';

export default {
    props: ['user'],
    data: () => ({
        loading: true,
        page: 0,
        param: { start: 0, count: 10 },
        problems: [],
        pageCount: 0,
        headers: [
            { text: '编号', value: 'id' },
            { text: '时间', value: 'judgeTime' },
            { text: '题目', value: 'problemName' },
            { text: '用户', value: 'userName' },
            { text: '语言', value: 'language' },
            { text: '结果', value: 'result' },
            { text: '总分', value: 'fullScore' }
        ]
    }),
    mounted: function () {
        setTitle('状态');
        if (!this.$route.params.page) {
            this.$router.push(this.$router.currentRoute.fullPath + '/1');
            this.page = 1;
        }
        else this.page = parseInt(this.$route.params.page);
        if (this.$route.params.pid) this.param['pid'] = parseInt(this.$route.params.pid);
        if (this.$route.params.cid) {
            this.param['cid'] = parseInt(this.$route.params.cid);
            this.headers.append([{ text: '比赛', value: 'contestId' }]);
        }
        if (this.$route.params.gid) {
            this.param['gid'] = parseInt(this.$route.params.gid);
            this.headers.append([{ text: '小组', value: 'contestId' }]);
        }
        Get('/Status/GetStatusCount', this.param)
            .then(res => res.text())
            .then(data => {
                this.pageCount = Math.ceil(data / 10);
                if (this.pageCount === 0) this.pageCount = 1;
                if (this.page > this.pageCount) this.page = this.pageCount;
                if (this.page <= 0) this.page = 1;
            })
            .catch(() => this.pageCount = 0);
    },
    watch: {
        page: function () {
            let baseRoute = '/Status';
            if (this.$route.params.gid) baseRoute = baseRoute + '/' + this.$route.params.gid;
            if (this.$route.params.cid) baseRoute = baseRoute + '/' + this.$route.params.cid;
            if (this.$route.params.pid) baseRoute = baseRoute + '/' + this.$route.params.pid;
            this.$router.push(baseRoute + '/' + this.page);
            this.loading = true;
            this.param.start = (this.page - 1) * 10;
            this.statuses = [];
            Get('/Status/GetStatusList', this.param)
                .then(res => res.json())
                .then(data => {
                    this.statuses = data;
                    this.loading = false;
                })
                .catch(() => {
                    this.statuses = [];
                    this.loading = false;
                });
        }
    },
    methods: {
        toResult: function (id) {
            this.$router.push('/Result/' + id.toString());
        },
        toProblem: function (id) {
            this.$router.push('/ProblemDetails/' + id.toString());
        },
        toUser: function (id) {
            this.$router.push('/Account/' + id.toString());
        },
        toContest: function (id) {
            this.$router.push('/ContestDetails/' + id.toString());
        },
        toGroup: function (id) {
            this.$router.push('/GroupDetails/' + id.toString());
        }
    }
};