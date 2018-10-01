import { setTitle } from '../../utilities/titleHelper';
import { Get } from '../../utilities/requestHelper';

export default {
    data: () => ({
        loading: true,
        page: 0,
        contests: [],
        pageCount: 0,
        headers: [
            {
                text: '编号',
                align: 'left',
                sortable: true,
                value: 'id'
            },
            { text: '名称', value: 'name' },
            { text: '开始时间', value: 'startTime' },
            { text: '结束时间', value: 'endTime' },
            { text: '题目数量', value: 'problemCount' },
            { text: '状态', value: 'status' }
        ]
    }),
    mounted: function () {
        setTitle('比赛');
        if (!this.$route.params.page) {
            this.$router.push('/Contest/1');
            this.page = 1;
        }
        else this.page = parseInt(this.$route.params.page);
        Get('/Contest/GetContestCount')
            .then(res => res.text())
            .then(data => {
                this.pageCount = Math.ceil(data / 10);
                if (this.page > this.pageCount)
                    this.page = this.pageCount;
            })
            .catch(() => this.pageCount = 0);
    },
    watch: {
        page: function () {
            this.$router.push('/Contest/' + this.page);
            this.loading = true;
            let param = { start: (this.page - 1) * 10, count: 10 };
            if (this.$route.params.gid) param['gid'] = this.$route.params.gid;
            this.contests = [];
            Get('/Contest/GetContestList', param)
                .then(res => res.json())
                .then(data => {
                    this.contests = data;
                    this.loading = false;
                })
                .catch(() => {
                    this.contests = [];
                    this.loading = false;
                });
        }
    },
    methods: {
        toDetails: function (id) {
            this.$router.push('/ContestDetails/' + id.toString());
        }
    }
};