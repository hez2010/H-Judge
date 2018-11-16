import { Get, Post } from '../../utilities/requestHelper';
import { setTitle } from '../../utilities/titleHelper';

export default {
    props: ['user'],
    data: () => ({
        result: {},
        loading: true,
        headers: [
            { text: '编号', value: 'id' },
            { text: '时间 (ms)', value: 'timeCost' },
            { text: '内存 (kb)', value: 'memoryCost' },
            { text: '退出代码', value: 'exitCode' },
            { text: '结果', value: 'result' },
            { text: '分数', value: 'score' },
            { text: '附加信息', value: 'extraInfo' }
        ],
        timer: null
    }),
    mounted: function () {
        setTitle('结果');
        let jid = this.$route.params.jid;
        Get('/Status/GetJudgeResult', { jid: jid })
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.result = data;
                    if (data.resultType <= 0) {
                        this.$nextTick(() => this.timer = setInterval(() => this.queryResult(jid), 5000));
                    }
                }
                else {
                    alert(data.errorMessage);
                }
                this.loading = false;
            })
            .catch(() => {
                alert('加载失败');
                this.loading = false;
            });
    },
    methods: {
        getProblemRouteParams: function (id) {
            return (this.result.groupId ? this.result.groupId.toString() + '/' : '') + (this.result.contestId ? this.result.contestId.toString() + '/' : '') + id.toString();
        },
        getContestRouteParams: function (id) {
            return (this.result.groupId ? this.result.groupId.toString() + '/' : '') + id.toString();
        },
        queryResult: function (jid) {
            Get('/Status/GetJudgeResult', { jid: jid })
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        this.result = data;
                        if (data.resultType > 0) {
                            clearInterval(this.timer);
                            this.timer = null;
                        }
                    }
                });
        },
        rejudge: function (jid) {
            Post('/Status/Rejudge', { jid: jid })
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        this.loading = true;
                        Get('/Status/GetJudgeResult', { jid: jid })
                            .then(res => res.json())
                            .then(data => {
                                if (data.isSucceeded) {
                                    this.result = data;
                                    if (data.resultType <= 0) {
                                        this.$nextTick(function () {
                                            this.timer = setInterval(() => this.queryResult(jid), 5000);
                                        });
                                    }
                                }
                                else {
                                    alert(data.errorMessage);
                                }
                                this.loading = false;
                            })
                            .catch(() => {
                                alert('加载失败');
                                this.loading = false;
                            });
                    }
                    else alert(data.errorMessage);
                })
                .catch(() => alert('请求失败'));
        },
        setPublic: function () {
            Post('/Status/SetResultVisibility', { judgeId: this.result.id, isPublic: this.result.isPublic })
                .then(res => res.json())
                .then(data => {
                    if (!data.isSucceeded) {
                        alert(data.errorMessage);
                        this.result.isPublic = !this.result.isPublic;
                    }
                })
                .catch(() => {
                    this.result.isPublic = !this.result.isPublic;
                    alert('请求失败');
                });
        }
    }
};