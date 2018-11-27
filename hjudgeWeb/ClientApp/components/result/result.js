import { Get, Post } from '../../utilities/requestHelper';
import { setTitle } from '../../utilities/titleHelper';
import { initializeObjects } from '../../utilities/initHelper';

export default {
    props: ['user', 'showSnack'],
    data: () => ({
        loading: true,
        headers: [
            { text: '编号', value: 'id' },
            { text: '时间 (ms)', value: 'timeCost' },
            { text: '内存 (kb)', value: 'memoryCost' },
            { text: '退出代码', value: 'exitCode' },
            { text: '结果', value: 'result' },
            { text: '分数', value: 'score' },
            { text: '附加信息', value: 'extraInfo' }
        ]
    }),
    mounted: function () {
        setTitle('结果');

        initializeObjects({
            result: {},
            timer: null
        }, this);

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
                    this.showSnack(data.errorMessage, 'error', 3000);
                }
                this.loading = false;
            })
            .catch(() => {
                this.showSnack('加载失败', 'error', 3000);
                this.loading = false;
            });
    },
    methods: {
        getProblemRouteParams: function (id) {
            if (id)
                return (this.result.groupId ? this.result.groupId.toString() + '/' : '') + (this.result.contestId ? this.result.contestId.toString() + '/' : '') + id.toString();
            else return '';
        },
        getContestRouteParams: function (id) {
            if (id)
                return (this.result.groupId ? this.result.groupId.toString() + '/' : '') + id.toString();
            else return '';
        },
        queryResult: function (jid) {
            Get('/Status/GetJudgeResult', { jid: jid })
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        this.result = data;
                        this.$forceUpdate();
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
                                    this.showSnack(data.errorMessage, 'error', 3000);
                                }
                                this.loading = false;
                            })
                            .catch(() => {
                                this.showSnack('加载失败', 'error', 3000);
                                this.loading = false;
                            });
                    }
                    else this.showSnack(data.errorMessage, 'error', 3000);
                })
                .catch(() => this.showSnack('请求失败', 'error', 3000));
        },
        setPublic: function () {
            Post('/Status/SetResultVisibility', { judgeId: this.result.id, isPublic: this.result.isPublic })
                .then(res => res.json())
                .then(data => {
                    if (!data.isSucceeded) {
                        this.showSnack(data.errorMessage, 'error', 3000);
                        this.result.isPublic = !this.result.isPublic;
                    }
                    else {
                        this.showSnack('设置成功', 'success', 3000);
                    }
                })
                .catch(() => {
                    this.result.isPublic = !this.result.isPublic;
                    this.showSnack('请求失败', 'error', 3000);
                });
        }
    }
};