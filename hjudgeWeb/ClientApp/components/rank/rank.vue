<template>
    <v-container>
        <v-card>
            <v-card-title primary-title>
                <h2>比赛排名</h2>
                <v-spacer></v-spacer>
                <div>
                    <v-tooltip bottom v-if="!loading && user.privilege >= 1 && user.privilege <= 3">
                        <v-btn @click="rejudge($route.params.cid, $route.params.gid)" icon slot="activator">
                            <v-icon color="primary">refresh</v-icon>
                        </v-btn>
                        <span>重新评测</span>
                    </v-tooltip>
                </div>
            </v-card-title>
            <v-card-text>
                <v-container v-if="loading">
                    <p>加载中...</p>
                </v-container>
                <v-container v-else>
                    <v-data-table :headers="headers"
                                  :items="loading ? [] : rankInfo"
                                  hide-actions>
                        <template slot="items" slot-scope="props">
                            <td>{{props.item.rank}}</td>
                            <td><router-link :to="{ path: '/Account/' + props.item.user.id }">{{props.item.user.name}}</router-link></td>
                            <td>{{props.item.fullScore}}</td>
                            <td>{{props.item.timeCost}}</td>
                            <td>{{props.item.penalty}}</td>
                            <td v-for="(item, index) in props.item.problemInfo" :key="index">
                                <div :class="item.isAccepted ? 'success--text' : item.submissionCount === 0 ? '' : 'red--text'">
                                    <v-layout justify-center>{{item.score}}</v-layout>
                                    <v-layout justify-center>{{item.timeCost}}</v-layout>
                                    <v-layout justify-center>{{item.submissionCount}} (-{{item.penaltyCount}})</v-layout>
                                </div>
                            </td>
                        </template>
                        <template slot="no-data">
                            <p>没有数据 :(</p>
                        </template>
                    </v-data-table>
                </v-container>
            </v-card-text>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./rank.js"></script>