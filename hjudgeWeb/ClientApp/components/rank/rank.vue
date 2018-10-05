<template>
    <v-container>
        <v-card>
            <v-card-title primary-title>
                <h2>比赛排名</h2>
            </v-card-title>
            <v-card-text>
                <v-container v-if="loading">
                    <p>加载中...</p>
                </v-container>
                <v-container v-else>
                    <v-data-table :headers="headers"
                                  :items="rankInfo"
                                  hide-actions>
                        <template slot="items" slot-scope="props">
                            <td>{{props.item.rank}}</td>
                            <td><a @click="toUser(props.item.user.id)">{{props.item.user.name}}</a></td>
                            <td>{{props.item.fullScore}}</td>
                            <td>{{props.item.timeCost}}</td>
                            <td>{{props.item.penalty}}</td>
                            <td v-for="item in props.item.problemInfo">
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