<template>
    <v-container>
        <v-card>
            <v-card-title primary-title>
                <h2>比赛列表</h2>
            </v-card-title>
            <v-card-text>
                <v-data-table :headers="headers"
                              :items="contests"
                              hide-actions>
                    <template slot="items" slot-scope="props">
                        <tr @click="toDetails(props.item.id)" style="cursor: pointer">
                            <td>{{ props.item.id }} <span v-if="props.item.hidden">(隐藏)</span></td>
                            <td>{{ props.item.name }}</td>
                            <td>{{ props.item.startTime }}</td>
                            <td>{{ props.item.endTime }}</td>
                            <td>{{ props.item.problemCount }}</td>
                            <td>{{ props.item.status }}</td>
                        </tr>
                    </template>
                    <template slot="no-data">
                        <p v-if="loading">正在加载...</p>
                        <p v-else>没有数据 :(</p>
                    </template>
                </v-data-table>
            </v-card-text>
            <v-card-action>
                <div class="text-xs-center">
                    <v-pagination circle v-model="page" :length="pageCount" :total-visible="7"></v-pagination>
                </div>
            </v-card-action>
        </v-card>
    </v-container>
</template>

<script src="./contest.js"></script>