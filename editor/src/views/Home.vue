<template>
  <div class="home">
    <button @click="getTables()">get Tables</button>
    <button @click="clear()">RemoveAll</button>
    <button @click="addPage({params:{link}})">add page</button>

    <input v-model="link" />
    {{tables}}
    <vue-json-to-table :data="{}"></vue-json-to-table>
  </div>
</template>

<script>
// @ is an alias to /src
import { mapState, mapActions } from "vuex";
import { AgGridVue } from "ag-grid-vue";

export default {
  name: "Home",
  data: () => {
    return {
      link: "",
    };
  },
  components: { AgGridVue },
  computed: mapState({
    tables: (state) => state.tables,
  }),
  methods: {
    ...mapActions(["getTables", "addPage", "clear"]),
  },
};
</script>
