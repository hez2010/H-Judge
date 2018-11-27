import { setTitle } from '../../utilities/titleHelper';
import { Get } from '../../utilities/requestHelper';
import { initializeObjects } from '../../utilities/initHelper';

export default {
    props: ['showSnack'],
    data: () => ({
        headers: [
            { text: '排名', value: 'rank' },
            { text: '用户', value: 'user' },
            { text: '总分', value: 'fullScore' },
            { text: '用时', value: 'timeCost' },
            { text: '罚时', value: 'penalty' }
        ],
        loading: true
    }),
    mounted: function () {
        setTitle('排名');

        initializeObjects({
            rankInfo: []
        }, this);

        let cid = this.$route.params.cid;
        let gid = this.$route.params.gid;
        if (!cid) cid = 0;
        if (!gid) gid = 0;
        Get('/Contest/GetRank', { gid: gid, cid: cid })
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    for (var i in data.problemInfo) {
                        let item = data.problemInfo[i];
                        this.headers = this.headers.concat([{ text: `${item.id} - ${item.name} (${item.acceptedCount}/${item.submissionCount})`, value: item.id }]);
                    }
                    for (var j in data.rankInfo) {
                        let item = data.rankInfo[j];
                        let r = {
                            rank: item.rank,
                            user: { name: item.userInfo.userName + (item.userInfo.name ? ` (${item.userInfo.name})` : ''), id: item.userInfo.id },
                            fullScore: item.fullScore,
                            timeCost: item.timeCost,
                            penalty: item.penalty,
                            problemInfo: {}
                        };
                        for (var p = 5; p < this.headers.length; p++) {
                            let index = this.headers[p].value;
                            let problemInfo = item.submitInfo[index];
                            if (problemInfo) {
                                r.problemInfo[index] = {
                                    isAccepted: problemInfo.isAccepted,
                                    penaltyCount: problemInfo.penaltyCount,
                                    score: problemInfo.score,
                                    submissionCount: problemInfo.submissionCount,
                                    timeCost: problemInfo.timeCost
                                };
                            } else {
                                r.problemInfo[index] = {
                                    isAccepted: false,
                                    penaltyCount: 0,
                                    score: 0,
                                    submissionCount: 0,
                                    timeCost: '00:00:00'
                                };
                            }
                        }
                        this.rankInfo = this.rankInfo.concat([r]);
                    }

                }
                else this.showSnack(data.errorMessage, 'error', 3000);
                this.loading = false;
            })
            .catch(() => {
                this.showSnack('加载失败', 'error', 3000);
                this.loading = false;
            });
    }
};