<template>
  <div class="container is-fluid mt-3">
    <div class="flex">
      <div class="column is-narrow editor-column">
        <editor class="editor" :Target="shema"></editor>
      </div>
      <div class="column">
        <div class="content-header">
          <b-field>
            <b-button @click="addPage({params:{link}})">add page</b-button>
            <b-input v-model="link" />
          </b-field>
        </div>
        <div class="content">
          <vueJsonToTable :data="tables"></vueJsonToTable>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
// @ is an alias to /src
import { mapState, mapActions } from "vuex";
import { AgGridVue } from "ag-grid-vue";
import vueJsonToTable from "../components/VueJsonToTable";
import editor from "../components/TableEditor";

export default {
  name: "Home",
  data: () => {
    return {
      link: "",
      shema: {
        Property: "main",
        Children: [
          {
            Property: "film",
            Children: [
              {
                Property: "Raiting",
                Children: [
                  {
                    Property: "Test",
                    Children: [],
                  },
                ],
              },
              {
                Property: "Reviews",
                Children: [],
              },
            ],
          },
          {
            Property: "menu",
            Children: [],
          },
        ],
      },
    };
  },
  components: { editor, vueJsonToTable },
  computed: mapState({
    tables: (state) => state.tables,
  }),
  methods: {
    ...mapActions(["getTables", "addPage", "clear"]),
  },
};
</script>
<style>
.mt-3 {
  margin-top: 20px;
}
.editor-column {
  min-width: 25em;
}
.content {
  height: 90vh;
  overflow-y: scroll;
  border: gray 2px solid;
}
.content-header {
  padding: 5px;
}
</style>