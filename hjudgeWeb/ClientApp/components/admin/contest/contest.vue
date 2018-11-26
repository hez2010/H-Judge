<template>
    <v-container>
        <v-card>
            <v-tabs right>
                <v-card-title primary-title>
                    <h2>比赛编辑</h2>
                </v-card-title>
                <v-spacer></v-spacer>
                <v-tab :key="1">
                    信息
                </v-tab>
                <v-tab :key="2">
                    描述
                </v-tab>
                <v-tab :key="3">
                    选项
                </v-tab>
                <v-tab-item :key="1">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else>
                            <v-form ref="basic" v-model="valid" lazy-validation>
                                <v-text-field v-model="contest.name"
                                              :rules="requireRules"
                                              label="比赛名称"
                                              required>
                                </v-text-field>
                                <v-layout>
                                    <label>开始时间：</label>
                                    <input v-model="contest.startTime"
                                           :rules="requireRules"
                                           required
                                           type="datetime-local" />
                                    <label>结束时间：</label>
                                    <input v-model="contest.endTime"
                                           :rules="requireRules"
                                           required
                                           type="datetime-local" />
                                </v-layout>
                                <v-text-field v-model="contest.problemSet"
                                              label="题目列表（以 ; 分隔）">
                                </v-text-field>
                                <v-text-field v-model="contest.password"
                                              label="进入密码">
                                </v-text-field>
                                <v-checkbox label="隐藏比赛"
                                            v-model="contest.hidden">
                                </v-checkbox>
                            </v-form>
                        </v-container>
                    </v-card-text>
                </v-tab-item>

                <v-tab-item :key="2">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else>
                            <textarea hidden id="editor" v-model="contest.description"></textarea>
                        </v-container>
                    </v-card-text>
                </v-tab-item>

                <v-tab-item :key="3">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else>
                            <v-radio-group label="比赛类型" v-model="contest.config.type">
                                <v-layout>
                                    <div>
                                        <v-radio key="0"
                                                 label="一般计时赛"
                                                 :value="0">
                                        </v-radio>
                                    </div>
                                    <div>
                                        <v-radio key="1"
                                                 label="最后提交赛"
                                                 :value="1">
                                        </v-radio>
                                    </div>
                                    <div>
                                        <v-radio key="2"
                                                 label="罚时计时赛"
                                                 :value="2">
                                        </v-radio>
                                    </div>
                                </v-layout>
                            </v-radio-group>
                            <v-radio-group label="结果反馈" v-model="contest.config.resultMode">
                                <v-layout>
                                    <div>
                                        <v-radio key="0"
                                                 label="即时反馈"
                                                 :value="0">
                                        </v-radio>
                                    </div>
                                    <div>
                                        <v-radio key="1"
                                                 label="赛后反馈"
                                                 :value="1">
                                        </v-radio>
                                    </div>
                                    <div>
                                        <v-radio key="2"
                                                 label="不反馈"
                                                 :value="2">
                                        </v-radio>
                                    </div>
                                </v-layout>
                            </v-radio-group>
                            <v-radio-group label="结果显示" v-model="contest.config.resultType">
                                <v-layout>
                                    <div>
                                        <v-radio key="0"
                                                 label="详细结果"
                                                 :value="0">
                                        </v-radio>
                                    </div>
                                    <div>
                                        <v-radio key="1"
                                                 label="简略结果"
                                                 :value="1">
                                        </v-radio>
                                    </div>
                                </v-layout>
                            </v-radio-group>
                            <v-radio-group label="计分模式" v-model="contest.config.scoreMode">
                                <v-layout>
                                    <div>
                                        <v-radio key="0"
                                                 label="全部计分"
                                                 :value="0">
                                        </v-radio>
                                    </div>
                                    <div>
                                        <v-radio key="1"
                                                 label="仅计 Accepted"
                                                 :value="1">
                                        </v-radio>
                                    </div>
                                </v-layout>
                            </v-radio-group>
                            <v-layout>
                                <div>
                                    <v-checkbox label="显示排名" v-model="contest.config.showRank"></v-checkbox>
                                </div>
                                <div>
                                    <v-checkbox label="启用封榜" v-model="contest.config.autoStopRank"></v-checkbox>
                                </div>
                            </v-layout>
                            <v-layout>
                                <div>
                                    <v-checkbox label="允许公开代码" v-model="contest.config.canMakeResultPublic"></v-checkbox>
                                </div>
                                <div>
                                    <v-checkbox label="允许参与讨论" v-model="contest.config.canDisscussion"></v-checkbox>
                                </div>
                            </v-layout>
                            <v-layout>
                                <v-text-field label="提交次数限制（0 为不限）" v-model="contest.config.submissionLimit" type="number"></v-text-field>
                                <v-text-field label="提交语言限制（以 ; 分隔）" v-model="contest.config.languages"></v-text-field>
                            </v-layout>
                        </v-container>
                    </v-card-text>
                </v-tab-item>
            </v-tabs>

            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="primary" :disabled="!isValid() || submitting || markdownEnabled" @click="save">保存</v-btn>
            </v-card-actions>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./contest.js"></script>