<template>
    <v-container>
        <v-card>
            <v-tabs right>
                <v-card-title primary-title>
                    <h2>评测结果</h2>
                </v-card-title>
                <v-spacer></v-spacer>
                <v-tab :key="1">
                    信息
                </v-tab>
                <v-tab :key="2">
                    提交
                </v-tab>
                <v-tab :key="3" v-if="result.resultType > 0">
                    详情
                </v-tab>
                <v-tab :key="4" v-if="result.rawType === 1 && (!!result.judgeResult.compileLog || !!result.judgeResult.staticCheckLog)">
                    日志
                </v-tab>
                <v-tab-item :key="1">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else>
                            <v-layout wrap>
                                <v-flex xs4>
                                    <strong>评测编号</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{result.id}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>评测时间</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{result.judgeTime}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>提交用户</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p><router-link :to="{ path: '/Account/' + result.userId }">{{result.userName}}</router-link></p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>题目名称</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p><router-link :to="{ path: '/ProblemDetails/' + getProblemRouteParams(result.problemId) }">{{result.problemName}}</router-link></p>
                                </v-flex>
                                <v-flex xs4 v-if="result.rawType === 1">
                                    <strong>选用语言</strong>
                                </v-flex>
                                <v-flex xs8 v-if="result.rawType === 1">
                                    <p>{{result.language}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>评测结果</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>
                                        <v-progress-circular indeterminate
                                                             color="primary"
                                                             v-if="result.resultType <= 0">
                                        </v-progress-circular>
                                        {{result.result}}
                                        <v-tooltip bottom v-if="!loading && result.resultType > 0 && user.privilege >= 1 && user.privilege <= 3">
                                            <v-btn @click="rejudge(result.id)" icon slot="activator">
                                                <v-icon color="primary">refresh</v-icon>
                                            </v-btn>
                                            <span>重新评测</span>
                                        </v-tooltip>
                                    </p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>所得总分</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{result.fullScore}}</p>
                                </v-flex>
                                <v-flex xs4 v-if="result.contestId !== 0">
                                    <strong>比赛名称</strong>
                                </v-flex>
                                <v-flex xs8 v-if="result.contestId !== 0">
                                    <p><router-link :to="{ path: '/ContestDetails/' + getContestRouteParams(result.contestId) }">{{result.contestName}}</router-link></p>
                                </v-flex>
                                <v-flex xs4 v-if="result.groupId !== 0">
                                    <strong>小组名称</strong>
                                </v-flex>
                                <v-flex xs8 v-if="result.groupId !== 0">
                                    <p><router-link :to="{ path: '/GroupDetails/' + result.groupId }">{{result.groupName}}</router-link></p>
                                </v-flex>
                            </v-layout>
                        </v-container>
                    </v-card-text>
                </v-tab-item>
                <v-tab-item :key="2">
                    <v-card-text>
                        <v-container v-if="loading"><p>加载中...</p></v-container>
                        <v-container v-else>
                            <div v-highlight>
                                <pre class="detail-field"><code>{{result.content}}</code></pre>
                            </div>
                        </v-container>
                    </v-card-text>
                </v-tab-item>
                <v-tab-item :key="3" v-if="result.resultType > 0">
                    <v-card-text>
                        <v-container v-if="loading"><p>加载中...</p></v-container>
                        <v-container v-else>
                            <v-data-table :headers="headers"
                                          :items="result.judgeResult.judgePoints"
                                          hide-actions
                                          v-if="result.judgeResult.judgePoints">
                                <template slot="items" slot-scope="props">
                                    <td>{{ props.index + 1 }}</td>
                                    <td>{{ props.item.timeCost }}</td>
                                    <td>{{ props.item.memoryCost }}</td>
                                    <td>{{ props.item.exitCode }}</td>
                                    <td>{{ props.item.result }}</td>
                                    <td>{{ props.item.score }}</td>
                                    <td>{{ props.item.extraInfo }}</td>
                                </template>
                                <template slot="no-data">
                                    <p>没有数据 :(</p>
                                </template>
                            </v-data-table>
                        </v-container>
                    </v-card-text>
                </v-tab-item>
                <v-tab-item :key="4" v-if="result.rawType === 1 && (!!result.judgeResult.compileLog || !!result.judgeResult.staticCheckLog)">
                    <v-card-text>
                        <v-container v-if="loading"><p>加载中...</p></v-container>
                        <v-container v-else>
                            <div v-if="!!result.judgeResult.compileLog" v-highlight>
                                <h4>编译日志</h4>
                                <pre class="detail-field"><code>{{result.judgeResult.compileLog}}</code></pre>
                            </div>
                            <div v-if="!!result.judgeResult.staticCheckLog" v-highlight>
                                <h4>静态检查</h4>
                                <pre class="detail-field"><code>{{result.judgeResult.staticCheckLog}}</code></pre>
                            </div>
                        </v-container>
                    </v-card-text>
                </v-tab-item>
            </v-tabs>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./result.js"></script>